﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Migoto.Log.Parser
{
    using ApiCalls;
    using ApiCalls.Draw;
    using Assets;
    using Slots;

    public class FrameAnalysis
    {
        private readonly StreamReader stream;
        private readonly Action<string> logger;

        private readonly Regex indexPattern = new Regex(@"^(?:(?'frame'\d)+\.)?(?'drawcall'\d{6})");
        private readonly Regex methodPattern = new Regex(@"(?'method'\w+)\((?'args'.*?)\)(?: hash=(?'hash'\w+))?");
        private readonly Regex methodArgPattern = new Regex(@"(?'name'\w+):(?'value'\w+)");
        private readonly Regex resourcePattern = new Regex(@"\s+(?'index'\w+)?: (?:view=(?'view'\w+) )?resource=(?'resource'\w+)(?: hash=(?'hash'\w+))?");
        private readonly Regex samplerPattern = new Regex(@"\s+(?'index'\w+): handle=(?'handle'\w+)");
        private readonly Regex dataPattern = new Regex(@"\s+data: \w+");
        private readonly Regex logicPattern = new Regex(@"3DMigoto (?'logic'.*)");

        private readonly Dictionary<string, Type> apiCallTypes = typeof(IApiCall).Assembly.GetTypes().Where(t => t.Is<IApiCall>() && !t.IsAbstract).ToDictionary(t => t.Name, t => t);
        private readonly Dictionary<Type, PropertyInfo> drawCallProps = typeof(DrawCall).GetProperties().Where(p => p.IsGeneric(typeof(ICollection<>)) || p.CanWrite).ToDictionary(p => p.PropertyType, p => p);
        private readonly Dictionary<Type, PropertyInfo> shaderContextProps = typeof(ShaderContext).GetProperties().Where(p => p.CanWrite).ToDictionary(p => p.PropertyType, p => p);

        public readonly Dictionary<string, int> apiCallsSkipped = new Dictionary<string, int>();
        public readonly Dictionary<string, int> frameSkipped = new Dictionary<string, int>();

        public const string Extension = ".txt";

        public List<Frame> Frames { get; } = new List<Frame>();
        public Dictionary<uint, Asset> Assets { get; } = new Dictionary<uint, Asset>();
        public Dictionary<ulong, Shader> Shaders { get; } = new Dictionary<ulong, Shader>();

        private uint frameNo = 0;
        private Frame frame = new Frame(0); // For Present post logic
        private uint drawCallNo = 0;
        private uint apiCallNo = 0;
        private DrawCall drawCall = new DrawCall(0, null);
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

            var line = 2;

            while (!stream.EndOfStream)
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
                line++;
            }
            LogUnhandledForDrawCall();
            LogUnhandledForFrame();

            SimplifyLogic();

            return true;
        }

        private void ParseLine(string line)
        {
            var apiCallMatch = methodPattern.Match(line);
            if (apiCallMatch.Success)
            {
                var indexMatch = indexPattern.Match(line);
                if (indexMatch.Success)
                    ProcessFrameAndDrawCall(indexMatch.Groups);

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
                (find:new Regex(@"[cC]onfigs\\(.*?)\.ini"), with: "$1"),
                // Combine command and inspection 
                (find:new Regex(@"(?<!pre|post) (\[.*?\] .*)(?<!else \{|true|false)(?:[\r\n]+\s+)(?!else|\[|\})([^ ].*)(?=[\r\n])"), with: " $1 : $2"),
                // Copy scope after pre / post
                (find: new Regex(@"(pre|post) {(?=[\r\n]+\s+(\[.*?\]))"), with: "$1 $2 {"),
                // Remove repeated scopes
                (find: new Regex(@"(?<!pre|post) (\[.*?\])"), with: ""),
                // Remove zero fractionals
                (find: new Regex(@"(\d+)\.0+\b"), with: "$1"),
                // Simplify assignment inspection
                (find: new Regex(@"(?<=:)( ini param override)? ="), with: ""),
                // Remove constant inspection
                (find: new Regex(@" = (-?\d+(?:\.\d+)?) : \1"), with: " = $1"),
                // Remove Empty Else-EndIf
                (find: new Regex(@"(?:([\r\n]\s+)else \{\1\})? endif"), with: ""),
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
                    frame = new Frame(thisFrameNo);
                    Frames.Add(frame);
                    // New frame must be new draw call, but for some reason 3Dmigoto doesn't increment the draw call number
                    InitNewDrawCall(thisDrawCallNo);
                }
                else if (thisDrawCallNo != drawCallNo)
                {
                    InitNewDrawCall(thisDrawCallNo);
                }
            }

            void InitNewDrawCall(uint thisDrawCallNo)
            {
                if (drawCall != null)
                    LogUnhandledForDrawCall();
                drawCallNo = thisDrawCallNo;
                apiCallNo = 0;
                drawCall = new DrawCall(thisDrawCallNo, drawCall);
                frame.DrawCalls.Add(drawCall);
            }
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

                        if (apiCall is IASetIndexBuffer indexBuffer)
                            asset = new Buffer();
                        else if (asset == null)
                            asset = new Unknown();

                        RegisterAsset(hash, asset, unknown);
                    }
                    assetSlot.UpdateAsset(asset);
                }
            }
            if (apiCallType.Is<IDraw>())
                apiCallType = typeof(IDraw);
            if (shaderType.HasValue && shaderContextProps.TryGetValue(apiCallType, out var property))
            {
                var shaderCtx = drawCall.Shader(shaderType.Value);
                apiCall.GetType().GetProperty(nameof(ShaderType))?.SetValue(apiCall, shaderType.Value);
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
            PropertyInfo? slots;
            Type slotType;

            string index = captures["index"].Value;
            var useList = apiCall is IMultiSlot && uint.TryParse(index, out var _);

            if (useList)
            {
                slots = apiCallType.GetProperties().FirstOrDefault(p => p.IsGeneric(typeof(ICollection<>)) && p.FirstType().Is<IResourceSlot>());
                if (slots == null)
                    throw new InvalidOperationException($"{apiCall.Name} doesn't have a slots property.");
                slotType = slots.FirstType();
            }
            else
            {
                slots = apiCallType.GetProperty(index) ?? apiCallType.GetProperties().OfType<IResource>().SingleOrDefault();
                if (slots == null || slots.PropertyType.IsInterface)
                    throw new InvalidOperationException($"{apiCall.Name} doesn't have a concrete slot property called {index}.");
                slotType = slots.PropertyType;
            }

            var slot = slotType.Construct<Resource>();
            PopulateResourceSlot(captures, slot);

            if (useList)
            {
                slot.SetFromString(nameof(Resource.Index), captures["index"].Value);
                slots.Add(apiCall, slot);
            }
            else
            {
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
                    asset = slot is ResourceView ? new Texture() : (Asset)new Buffer();
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
            if (frameSkipped.Any())
                logger($"Frame: {frameNo} Summary");

            foreach (var method in frameSkipped)
                logger($"  {method.Value}x {method.Key} not supported");

            frameSkipped.Clear();
        }
    }
}
