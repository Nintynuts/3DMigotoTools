namespace Migoto.Log.Parser;

using ApiCalls;
using ApiCalls.Draw;
using Assets;
using Slots;

public class FrameAnalysis
{
    private readonly StreamReader stream;
    private readonly Action<string> logger;

    private readonly Regex indexPattern = new(@"^(?:(?'frame'\d)+\.)?(?'drawcall'\d{6})");
    private readonly Regex methodPattern = new(@"(?'method'\w+)\((?'args'.*?)\)(?: hash=(?'hash'\w+))?");
    private readonly Regex methodArgPattern = new(@"(?'name'\w+):(?'value'\w+)");
    private readonly Regex resourcePattern = new(@"\s+(?'index'\w+)?: (?:view=(?'view'\w+) )?resource=(?'resource'\w+)(?: hash=(?'hash'\w+))?");
    private readonly Regex samplerPattern = new(@"\s+(?'index'\w+): handle=(?'handle'\w+)");
    private readonly Regex dataPattern = new(@"\s+data: \w+");
    private readonly Regex logicPattern = new("3DMigoto (?'logic'.*)");

    private readonly Dictionary<string, Type> apiCallTypes = typeof(IApiCall).Assembly.GetTypes().Where(t => t.Is<IApiCall>() && !t.IsAbstract).ToDictionary(t => t.Name, t => t);
    private readonly Dictionary<Type, PropertyInfo> drawCallProps = typeof(DrawCall).GetProperties().Where(p => p.IsGeneric(typeof(ICollection<>)) || p.CanWrite).ToDictionary(p => p.PropertyType, p => p);
    private readonly Dictionary<Type, PropertyInfo> shaderContextProps = typeof(ShaderContext).GetProperties().Where(p => p.CanWrite).ToDictionary(p => p.PropertyType, p => p);

    private readonly Dictionary<string, int> apiCallsSkipped = new();
    private readonly Dictionary<string, int> frameSkipped = new();

    public const string Extension = ".txt";

    public List<Frame> Frames { get; } = new List<Frame>();
    public Dictionary<uint, Asset> Assets { get; } = new Dictionary<uint, Asset>();
    public Dictionary<ulong, Shader> Shaders { get; } = new Dictionary<ulong, Shader>();

    private uint frameNo = 1;
    private Frame frame = new(1);
    private uint drawCallNo = 1;
    private uint apiCallNo = 0;
    private uint drawCallBase = 0;
    private DrawCall drawCall = new(0, null);
    private IApiCall? apiCall = null;

    public FrameAnalysis(StreamReader stream, Action<string> logger)
    {
        this.stream = stream;
        this.logger = logger;
        Frames.Add(frame);
        frame.DrawCalls.Add(drawCall);
    }

    public bool Parse()
    {
        var header = stream.ReadLine();
        if (header?.StartsWith("analyse_options") != true)
            return false;

        for (var line = 2; !stream.EndOfStream; line++)
        {
            try
            {
                try
                {
                    if (stream.ReadLine() is { } content)
                        ParseLine(content);
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }
            }
            catch (Exception e)
            {
                logger($"Exception parsing line {line}: {e.Message}");
            }
        }
        LogUnhandledForDrawCall();
        LogUnhandledForFrame();

        SimplifyLogic();

        return true;
    }

    private void ParseLine(string line)
    {
        var indexMatch = indexPattern.Match(line);
        if (indexMatch.Success)
            ProcessFrameAndDrawCall(indexMatch.Groups);

        var apiCallMatch = methodPattern.Match(line);
        if (apiCallMatch.Success)
        {
            // If we have 'logic' written out before any API calls, then it's the [Present] logic.
            if (drawCall.Logic?.Length > 0)
                InitNewDrawCall(drawCallNo);

            ProcessApiCall(apiCallMatch.Groups);
            return;
        }
        var slotMatch = resourcePattern.Match(line);
        if (slotMatch.Success)
        {
            ProcessResourceSlot(slotMatch.Groups);
            return;
        }
        var samplerMatch = samplerPattern.Match(line);
        if (samplerMatch.Success)
        {
            ProcessSamplerSlot(samplerMatch.Groups);
            return;
        }
        if (dataPattern.Match(line).Success)
            return; // We don't handle this
        var logicMatch = logicPattern.Match(line);
        if (logicMatch.Success)
        {
            if (!logicMatch.Groups["logic"].Value.StartsWith("Dumping"))
                drawCall.Logic += logicMatch.Groups["logic"].Value + "\n";
            return;
        }
        throw new ArgumentException($"Unrecognised line: {line}");
    }

    private void SimplifyLogic()
    {
        var replacements = new List<(Regex find, string with)> {
            //Simplify scope name
            (find:new Regex(@"((?:[cC]ommand[lL]ist|[oO]verride|[sS]hader)\\)(?:[cC]onfigs\\)?(.*?)\.ini\\_?"), with: "$1$2\\"),
            // Move ini param override note after value into brackets
            (find: new Regex("(ini param override) (= .*)"), with: "$2 // $1"),
            // Move assignment type note with after assignment
            (find:new Regex(@"[\r\n]+\s+(?=copying by|performing)"), with: " // "),
            // Add space before if inspection
            (find:new Regex(@"(?<=\)):"), with: " //"),
            // Combine command and inspection 
            (find:new Regex(@"[\r\n]+\s+= "), with: " // "),
            // Copy scope after pre / post
            (find: new Regex(@"\b(pre|post)\b \{(?=[\r\n\s]+(\[.*?\]))"), with: "$1 $2 {"),
            // Remove repeated scopes
            (find: new Regex(@"(?<!\b(?:pre|post)\b) (\[.*?\])"), with: ""),
            // Remove zero fractionals
            (find: new Regex(@"(\d+)\.0+\b"), with: "$1"),
            // Remove constant inspection
            (find: new Regex(@" = (-?\d+(?:\.\d+)?) // \1"), with: " = $1"),
            // Remove double comment
            (find: new Regex("(// .*? )//"), with: "$1"),
            // Remove Empty Else-EndIf and EndIf
            (find: new Regex(@"(?:([\r\n]\s+)else \{\1\})? endif"), with: ""),
            // Combine Run and Command path
            (find: new Regex(@"(?<=run )= ((?:builtin)?commandlist|customshader)(?:_|\\)?(.*)[\r\n\s]+(?:pre|post)?\s(?=\[\1(?:\\\w+)?\\?\2)"), with: ""),
            // Simplify to += or -=
            (find: new Regex(@"(\$\w+) = \1 ([+-])"), with:"$1 $2="),
        };

        foreach (var dc in Frames.SelectMany(f => f.DrawCalls))
        {
            if (dc.Logic != null)
                replacements.ForEach(r => dc.Logic = r.find.Replace(dc.Logic, r.with));
        }
    }

    private void ProcessFrameAndDrawCall(GroupCollection captures)
    {
        if (uint.TryParse(captures["drawcall"].Value, out var thisDrawCallNo))
        {
            if (captures["frame"].Success && uint.TryParse(captures["frame"].Value, out var thisFrameNo) && thisFrameNo != frameNo)
            {
                LogUnhandledForFrame();
                frameNo = thisFrameNo;
                drawCallBase = drawCallNo - 1;
                frame = new Frame(thisFrameNo);
                Frames.Add(frame);
                // New frame must be new draw call, but for some reason 3Dmigoto doesn't increment the draw call number
                InitNewDrawCall(thisDrawCallNo, newFrame: true);
            }
            else if (thisDrawCallNo != drawCallNo)
            {
                InitNewDrawCall(thisDrawCallNo);
            }
        }
    }

    void InitNewDrawCall(uint thisDrawCallNo, bool newFrame = false)
    {
        if (drawCall != null)
            LogUnhandledForDrawCall();
        drawCallNo = thisDrawCallNo;
        apiCallNo = 0;
        drawCall = newFrame ? new(0, null) : new(thisDrawCallNo - drawCallBase, drawCall);
        frame.DrawCalls.Add(drawCall);
    }

    private void ProcessApiCall(GroupCollection captures)
    {
        var methodName = captures["method"].Value;
        if (!(apiCallTypes.TryGetValue(methodName, out var apiCallType)
            || apiCallTypes.TryGetValue(methodName[2..], out apiCallType)))
        {
            RecordUnhandled(methodName);
            return;
        }
        ShaderTypes.FromLetter.TryGetValue(methodName[0], out ShaderType? shaderType);
        apiCall = apiCallType.Construct<IApiCall>(apiCallNo);
        apiCallNo++;

        var argsMatches = methodArgPattern.Match(captures["args"].Value);
        while (argsMatches.Success)
        {
            string name = argsMatches.Groups["name"].Value;
            if (shaderType.HasValue)
                name = name.Replace(shaderType.Value.ToString(), "");
            apiCall.SetFromString(name, argsMatches.Groups["value"].Value);
            argsMatches = argsMatches.NextMatch();
        }
        var hex = captures["hash"];
        if (hex.Success)
        {
            if (apiCall is SetShader setShader && shaderType != null)
            {
                var hash = ulong.Parse(hex.Value, NumberStyles.HexNumber);
                if (!Shaders.TryGetValue(hash, out var shader))
                {
                    shader = new Shader(shaderType.Value) { Hash = hash };
                    Shaders.Add(hash, shader);
                }
                shader.References.Add(drawCall);
                setShader.Shader = shader;
            }
            else if (apiCall is IAssetSlot assetSlot)
            {
                var hash = uint.Parse(hex.Value, NumberStyles.HexNumber);
                if (!Assets.TryGetValue(hash, out var asset) || asset is Unknown)
                {
                    var unknown = asset as Unknown;
                    if (apiCall is IASetIndexBuffer)
                        asset = new Buffer();
                    else
                        asset ??= new Unknown();

                    RegisterAsset(hash, asset, unknown);
                }
                assetSlot.UpdateAsset(asset);
            }
        }
        if (apiCallType.Is<IDraw>())
            apiCallType = typeof(IDraw);
        if (shaderType != null && shaderContextProps.TryGetValue(apiCallType, out var property))
        {
            if (apiCall is IShaderCall shaderCall)
                shaderCall.ShaderType = shaderType.Value;

            var shaderCtx = drawCall.Shaders[shaderType.Value];
            property.SetTo(shaderCtx, apiCall);
            //apiCall = property.GetFrom<IApiCall>(shaderCtx);
        }
        else if (drawCallProps.TryGetValue(apiCallType, out property))
        {
            property.SetTo(drawCall, apiCall);
            //apiCall = property.GetFrom<IApiCall>(drawCall);
        }
        else if (drawCallProps.TryGetValue(typeof(ICollection<>).MakeGenericType(apiCallType), out var listProperty))
        {
            listProperty.Add(drawCall, apiCall);
        }
        else
        {
            throw new InvalidOperationException($"DrawCall missing property for {methodName}");
        }
    }

    private void ProcessResourceSlot(GroupCollection captures)
    {
        if (apiCall is null)
            return;

        if (apiCall is IResource resource)
        {
            PopulateResourceSlot(captures, resource);
            return;
        }

        var apiCallType = apiCall.GetType();

        string index = captures["index"].Value;

        if (apiCall is IMultiSlot<IResourceSlot> multislot && uint.TryParse(index, out var _))
        {
            var slots = Array.Find(apiCallType.GetProperties(), p => p.IsGeneric(typeof(ICollection<>)) && p.FirstType().Is<IResourceSlot>());
            if (slots == null)
                throw new InvalidOperationException($"{apiCall.Name} doesn't have a slots property.");
            var slotType = slots.FirstType();

            var slot = slotType.Construct<IResourceSlot>();
            PopulateResourceSlot(captures, slot);

            slot.SetFromString(nameof(Resource.Index), captures["index"].Value);
            slots.Add(apiCall, slot);
        }
        else
        {
            var slots = apiCallType.GetProperty(index) ?? apiCallType.GetProperties().OfType<Resource>().SingleOrDefault();
            if (slots?.PropertyType.IsInterface != false)
                throw new InvalidOperationException($"{apiCall.Name} doesn't have a concrete slot property called {index}.");
            var slotType = slots.PropertyType;

            var slot = slotType.Construct<Resource>();
            PopulateResourceSlot(captures, slot);

            slot.SetOwner(apiCall);
            slots.SetTo(apiCall, slot);
        }
    }

    private void PopulateResourceSlot(GroupCollection captures, IResource slot)
    {
        var view = captures["view"];
        if (view.Success)
            slot.SetFromString(nameof(ResourceView.View), view.Value);

        var resourcePtr = captures["resource"].Value;
        slot.SetFromString(nameof(Resource.Pointer), resourcePtr);

        if (captures["hash"].Success)
        {
            var hex = captures["hash"].Value;
            var hash = uint.Parse(hex, NumberStyles.HexNumber);
            if (!Assets.TryGetValue(hash, out var asset) || asset is Unknown)
            {
                var unknown = asset as Unknown;
                asset = slot is ResourceView ? new Texture() : new Buffer();
                RegisterAsset(hash, asset, unknown);
            }

            slot.UpdateAsset(asset);
        }
    }

    private void RegisterAsset(uint hash, Asset asset, Unknown? unknown = null)
    {
        asset.Hash = hash;

        if (unknown != null)
        {
            unknown.ReplaceWith(asset);
            Assets[hash] = asset;
        }
        else
        {
            Assets.Add(hash, asset);
        }
    }

    private void ProcessSamplerSlot(GroupCollection captures)
    {
        if (apiCall is not SetSamplers samplers)
            throw new ArgumentException("Sampler slots without preceding SetSampler");

        var sampler = new Sampler();
        sampler.SetFromString(nameof(Sampler.Handle), captures["handle"].Value);
        samplers.Samplers.Add(sampler);
    }

    private void RecordUnhandled(string methodName)
    {
        if (!apiCallsSkipped.ContainsKey(methodName))
            apiCallsSkipped[methodName] = 0;
        apiCallsSkipped[methodName]++;

        if (!frameSkipped.ContainsKey(methodName))
            frameSkipped[methodName] = 0;
        frameSkipped[methodName]++;
    }

    private void LogUnhandledForDrawCall()
    {
        var messages = apiCallsSkipped.Select(call => $"{call.Value}x {call.Key} not supported")
            .Concat(drawCall.MergeWarnings)
            .Concat(drawCall.Collisions);

        if (!messages.Any())
            return;

        logger($"Frame: {frameNo} Call: {drawCallNo} Summary");

        messages.ForEach(msg => logger("  " + msg));

        apiCallsSkipped.Clear();
    }

    private void LogUnhandledForFrame()
    {
        if (frameSkipped.Count > 0)
            logger($"Frame: {frameNo} Summary");

        foreach (var method in frameSkipped)
            logger($"  {method.Value}x {method.Key} not supported");

        frameSkipped.Clear();
    }
}
