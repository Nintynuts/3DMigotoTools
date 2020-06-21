﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.DriverCall.Draw;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser
{
    public class Parser
    {
        private readonly StreamReader stream;
        private readonly Action<string> logger;

        private readonly Regex indexPattern = new Regex(@"^(?:(?'frame'\d)+\.)?(?'drawcall'\d{6})");
        private readonly Regex methodPattern = new Regex(@"(?'method'\w+)\((?'args'.*?)\)(?: hash=(?'hash'\w+))?");
        private readonly Regex methodArgPattern = new Regex(@"(?'name'\w+):(?'value'\w+)");
        private readonly Regex resourcePattern = new Regex(@"\s+(?'index'\w+)?: (?:view=(?'view'\w+) )?resource=(?'resource'\w+)(?: hash=(?'hash'\w+))?");
        private readonly Regex samplerPattern = new Regex(@"\s+(?'index'\w+): handle=(?'handle'\w+)");
        private readonly Regex logicPattern = new Regex(@"3DMigoto (?'logic'.*)");

        private readonly Dictionary<string, Type> driverCallTypes = typeof(DriverCall.Base).Assembly.GetTypes().Where(t => typeof(DriverCall.Base).IsAssignableFrom(t) && !t.IsAbstract).ToDictionary(t => t.Name, t => t);
        private readonly Dictionary<Type, PropertyInfo> drawCallProps = typeof(DrawCall).GetProperties().Where(p => p.IsGeneric(typeof(ICollection<>)) || p.CanWrite).ToDictionary(p => p.PropertyType, p => p);
        private readonly MethodInfo shader = typeof(DrawCall).GetMethod(nameof(DrawCall.Shader));
        private readonly Dictionary<Type, PropertyInfo> shaderContextProps = typeof(ShaderContext).GetProperties().Where(p => p.CanWrite).ToDictionary(p => p.PropertyType, p => p);

        private static ShaderType GetShaderType(PropertyInfo p)
            => Enums.Parse<ShaderType>(p.Name.Substring(0, p.Name.Length - "Shader".Length));

        private readonly Dictionary<char, ShaderType> ShaderTypes
            = Enums.Values<ShaderType>().ToDictionary(s => s.ToString()[0], s => s);

        public readonly Dictionary<string, int> driverCallSkipped = new Dictionary<string, int>();
        public readonly Dictionary<string, int> frameSkipped = new Dictionary<string, int>();

        public List<Frame> Frames { get; } = new List<Frame>();
        public Dictionary<string, Asset.Base> Assets { get; } = new Dictionary<string, Asset.Base>();
        public Dictionary<string, Shader> Shaders { get; } = new Dictionary<string, Shader>();

        private uint frameNo = 0;
        private Frame frame = new Frame(0); // For Present post logic
        private uint drawCallNo = 0;
        private uint driverCallNo = 0;
        private DrawCall drawCall = new DrawCall(0, null);
        private DriverCall.Base driverCall = null;

        public Parser(StreamReader stream, Action<string> logger)
        {
            this.stream = stream;
            this.logger = logger;
            Frames.Add(frame);
            frame.DrawCalls.Add(drawCall);
        }

        public List<Frame> Parse()
        {
            // analysis_options
            stream.ReadLine();
            var line = 2;

            while (!stream.EndOfStream)
            {
                try
                {
                    try
                    {
                        ParseLine(stream.ReadLine());
                    }
                    catch (TargetInvocationException tie)
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

            return Frames;
        }

        private void ParseLine(string line)
        {
            var driverCallMatch = methodPattern.Match(line);
            if (driverCallMatch.Success)
            {
                var indexMatch = indexPattern.Match(line);
                if (indexMatch.Success)
                    ProcessFrameAndDrawCall(indexMatch.Groups);

                ProcessDriverCall(driverCallMatch.Groups);
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

            Frames.SelectMany(f => f.DrawCalls).Where(dc => dc.Logic != null).ForEach(dc => replacements.ForEach(r => dc.Logic = r.find.Replace(dc.Logic, r.with)));
        }

        private void ProcessFrameAndDrawCall(GroupCollection captures)
        {
            if (captures["frame"].Success && uint.TryParse(captures["frame"].Value, out var thisFrameNo) && thisFrameNo != frameNo)
            {
                LogUnhandledForFrame();
                frameNo = thisFrameNo;
                frame = new Frame(thisFrameNo);
                drawCall = null; // sever fallback to previous frame
                Frames.Add(frame);
            }
            if (uint.TryParse(captures["drawcall"].Value, out var thisDrawCallNo) && thisDrawCallNo != drawCallNo)
            {
                if (drawCall != null)
                    LogUnhandledForDrawCall();
                drawCallNo = thisDrawCallNo;
                driverCallNo = 0;
                drawCall = new DrawCall(thisDrawCallNo, drawCall);
                frame.DrawCalls.Add(drawCall);
            }
        }

        private void ProcessDriverCall(GroupCollection captures)
        {
            var methodName = captures["method"].Value;
            if (!(driverCallTypes.TryGetValue(methodName, out var driverCallType)
                || driverCallTypes.TryGetValue(methodName.Substring(2), out driverCallType)))
            {
                RecordUnhandled(methodName);
                return;
            }
            ShaderTypes.TryGetValue(methodName[0], out ShaderType? shaderType);
            driverCall = driverCallType.Construct<DriverCall.Base>(driverCallNo, drawCall);
            driverCallNo++;

            var argsMatches = methodArgPattern.Match(captures["args"].Value);
            while (argsMatches.Success)
            {
                string name = argsMatches.Groups["name"].Value;
                if (shaderType.HasValue)
                    name = name.Replace(shaderType.Value.ToString(), "");
                driverCall.SetFromString(name, argsMatches.Groups["value"].Value);
                argsMatches = argsMatches.NextMatch();
            }
            var hash = captures["hash"];
            if (hash.Success)
            {
                if (driverCall is SetShader setShader)
                {
                    if (!Shaders.TryGetValue(hash.Value, out var shader))
                    {
                        shader = new Shader(shaderType.Value);
                        shader.SetFromString(nameof(Shader.Hash), hash.Value);
                        Shaders.Add(hash.Value, shader);
                    }
                    shader.References.Add(drawCall);
                    setShader.Shader = shader;
                }
                else
                {
                    var assetProp = driverCall.AssetProperty;
                    if (!Assets.TryGetValue(hash.Value, out var asset) || asset is Unknown)
                    {
                        var unknown = asset as Unknown;

                        if (assetProp.PropertyType != typeof(Asset.Base))
                            asset = assetProp.PropertyType.Construct<Asset.Base>();
                        else if (unknown == null)
                            asset = new Unknown();

                        RegisterAsset(hash.Value, asset, unknown);
                    }
                    asset.Uses.Add(driverCall as IResource);
                    driverCall.Set(assetProp, asset);
                }
            }
            if (typeof(IDraw).IsAssignableFrom(driverCallType))
                driverCallType = typeof(IDraw);
            if (shaderType.HasValue && shaderContextProps.TryGetValue(driverCallType, out var property))
            {
                var shaderCtx = drawCall.Shader(shaderType.Value);
                driverCall.GetType().GetProperty(nameof(ShaderType))?.SetValue(driverCall, shaderType.Value);
                shaderCtx.Set(property, driverCall);
                driverCall = shaderCtx.Get<DriverCall.Base>(property);
            }
            else if (drawCallProps.TryGetValue(driverCallType, out property))
            {
                drawCall.Set(property, driverCall);
                driverCall = drawCall.Get<DriverCall.Base>(property);
            }
            else if (drawCallProps.TryGetValue(typeof(ICollection<>).MakeGenericType(driverCallType), out var listProperty))
            {
                drawCall.Add(listProperty, driverCall);
            }
            else
            {
                throw new InvalidOperationException($"DrawCall missing property for {methodName}");
            }
        }

        private void ProcessResourceSlot(GroupCollection captures)
        {
            string index = captures["index"].Value;
            var useList = uint.TryParse(index, out var _);

            PropertyInfo slots;
            Type slotType;

            if (useList)
            {
                slots = driverCall.SlotsProperty;
                if (slots == null)
                    throw new InvalidOperationException($"{driverCall.GetType().Name} doesn't have a slots property.");
                slotType = slots.PropertyType.GetGenericArguments()[0];
            }
            else
            {
                slots = driverCall.GetType().GetProperty(index);
                slots ??= driverCall.GetType().GetProperties().SingleOrDefault(p => typeof(Slot.Base).IsAssignableFrom(p.PropertyType));
                if (slots == null)
                    throw new InvalidOperationException($"{driverCall.GetType().Name} doesn't have a slot property called {index}.");
                slotType = slots.PropertyType;
            }
            var slot = slotType.Construct<Resource>(driverCall);

            var view = captures["view"];
            if (view.Success)
                slot.SetFromString(nameof(ResourceView.View), view.Value);

            var resourcePtr = captures["resource"].Value;
            slot.SetFromString(nameof(Resource.Pointer), resourcePtr);

            if (captures["hash"].Success)
            {
                var hash = captures["hash"].Value;
                if (!Assets.TryGetValue(hash, out var asset) || asset is Unknown)
                {
                    var unknown = asset as Unknown;

                    if (slotType == typeof(ResourceView))
                        asset = new Texture();
                    else
                        asset = new ConstantBuffer();

                    RegisterAsset(hash, asset, unknown);
                }

                slot.Set(slotType.GetProperty(nameof(Resource.Asset)), asset);
                asset.Uses.Add(slot);
            }

            if (useList)
            {
                slot.SetFromString(nameof(Resource.Index), captures["index"].Value);
                driverCall.Add(slots, slot);
            }
            else
            {
                driverCall.Set(slots, slot);
            }
        }

        private void RegisterAsset(string hash, Asset.Base asset, Unknown unknown = null)
        {
            asset.SetFromString(nameof(Asset.Base.Hash), hash);

            if (unknown != null)
            {
                asset.Uses.AddRange(unknown.Uses);
                unknown.Uses.ForEach(s => s.UpdateAsset(asset));
                Assets[hash] = asset;
            }
            else
            {
                Assets.Add(hash, asset);
            }
        }

        private void ProcessSamplerSlot(GroupCollection captures)
        {
            var samplerSlots = driverCall.SlotsProperty;
            var sampler = new Sampler(driverCall);

            var handle = captures["handle"].Value;
            sampler.SetFromString(nameof(Sampler.Handle), handle);
            driverCall.Add(samplerSlots, sampler);
        }

        private void RecordUnhandled(string methodName)
        {
            if (!driverCallSkipped.ContainsKey(methodName))
                driverCallSkipped[methodName] = 0;
            driverCallSkipped[methodName]++;

            if (!frameSkipped.ContainsKey(methodName))
                frameSkipped[methodName] = 0;
            frameSkipped[methodName]++;
        }

        private void LogUnhandledForDrawCall()
        {
            var messages = driverCallSkipped.Select(call => $"{call.Value}x {call.Key} not supported")
                .Concat(drawCall.MergeWarnings)
                .Concat(drawCall.Collisions);

            if (!messages.Any())
                return;

            logger($"Frame: {frameNo} Call: {drawCallNo} Summary");

            messages.ForEach(msg => logger("  " + msg));

            driverCallSkipped.Clear();
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
