using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Migoto.Config;
    using Migoto.Log.Parser.ApiCalls;
    using Migoto.ShaderFixes;

    using Parser;
    using Parser.Slots;

    public class MigotoData
    {
        public const string D3DX = "d3dx.ini";
        public DrawCallColumns ColumnGroups { get; private set; }
        public List<(ShaderType type, ShaderColumns columns)> ShaderColumns { get; } = new();
        public FrameAnalysis? FrameAnalysis { get; private set; }
        public Config Config { get; } = new Config();
        public ShaderFixes ShaderFixes { get; } = new ShaderFixes();

        private readonly IUserInterface ui;
        private bool metadataLoaded;

        internal MigotoData(IUserInterface ui) => this.ui = ui;

        public bool LoadLog(string validFilePath, Action<string> logger)
        {
            ui.Event("Waiting for log to be readable...");
            using var frameAnalysisFile = IOHelpers.TryReadFile(validFilePath)!;
            ui.Event("Reading log...");
            FrameAnalysis = new FrameAnalysis(frameAnalysisFile, logger);

            if (!FrameAnalysis.Parse())
            {
                logger("Provided file is not a 3DMigoto FrameAnalysis log file");
                return false;
            }
            if (!metadataLoaded)
            {
                var possibleRoot = Directory.GetParent(validFilePath)?.Parent;
                if (possibleRoot?.GetFiles(D3DX).Any() == true)
                    GetMetadata(possibleRoot.FullName);
            }
            LinkOverrides(FrameAnalysis);
            LinkShaderFixes(FrameAnalysis);

            return true;
        }

        public bool GetColumnSelection(IEnumerable<string>? cmdColumns = null)
        {
            ColumnGroups = DrawCallColumns.Index;

            string initial = cmdColumns?.Delimit(' ') ?? string.Empty;

            return ui.GetValid("column selection (default: IA VS PS OM Logic)", initial, out initial, columnStr =>
            {
                var columnSelection = columnStr.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (columnSelection.Count == 0 || columnSelection.First() == string.Empty)
                    columnSelection.AddRange(new[] { "All", "VS", "PS" });

                var shaderColumns = new List<(ShaderType type, ShaderColumns columns)>();

                foreach (var column in columnSelection)
                {
                    try
                    {
                        var token = column.ToUpper();
                        var tokens = token.Split('-');
                        if (tokens.Length > 1)
                            shaderColumns.Add((ShaderTypes.FromLetter[tokens[0][0]], Enums.Parse<ShaderColumns>(tokens[1])));
                        else if (token.Last() == 'S')
                            shaderColumns.Add((ShaderTypes.FromLetter[token[0]], Converter.ShaderColumns.All));
                        else
                            ColumnGroups |= Enums.Parse<DrawCallColumns>(column);
                    }
                    catch
                    {
                        return (false, $"Failed to parse column: '{column}'", columnStr.Replace(column, "").Replace("  ", " "));
                    }
                }

                // Consolidate duplicate entries, just in case!
                ShaderColumns.Clear();
                ShaderColumns.AddRange(shaderColumns.GroupBy(s => s.type).Select(s => (s.Key, s.Select(c => c.columns).Aggregate((a, b) => a | b))));
                return (true, "", columnStr);
            });
        }

        public void GetMetadata(string rootPath)
        {
            ui.Status("Reading metadata from ini config files...");
            Config.Read(Path.Combine(rootPath, D3DX));
            ui.Event($"TextureOverrides: {Config.TextureOverrides.Count}\nShaderOverrides: {Config.ShaderOverrides.Count}");
            ui.Status("Reading metadata from shader fixes...");
            ShaderFixes.Scrape(rootPath);
            ui.Event($"Shaders: {ShaderFixes.ShaderNames.Count}\nTexture Registers: {ShaderFixes.Textures.Count}\nConstant Buffer Registers {ShaderFixes.ConstantBuffers.Count}");
            metadataLoaded = true;
        }

        public void LinkOverrides(FrameAnalysis frameAnalysis)
        {
            Config.TextureOverrides.ForEach(to =>
            {
                if (frameAnalysis.Assets.TryGetValue(to.Hash, out var asset))
                    asset.Override = to;
            });
            Config.ShaderOverrides.ForEach(so =>
            {
                if (frameAnalysis.Shaders.TryGetValue(so.Hash, out var shader))
                    shader.Override = so;
            });
        }

        public void LinkShaderFixes(FrameAnalysis frameAnalysis)
        {
            ShaderFixes.ShaderNames.ForEach(sf =>
            {
                if (frameAnalysis.Shaders.TryGetValue(sf.Hash, out var shader))
                    shader.Fix = sf;
            });
            LinkRegisters(frameAnalysis, ShaderFixes.Textures, s => s.SetShaderResources?.ResourceViews);
            LinkRegisters(frameAnalysis, ShaderFixes.ConstantBuffers, s => s.SetConstantBuffers?.ConstantBuffers);
        }

        private static void LinkRegisters(FrameAnalysis frameAnalysis, List<ShaderUsage<Register>> registers, Func<ShaderContext, IEnumerable<IResourceSlot>?> resourceSelector)
        {
            registers
            .ForEach(reg => reg.Hashes.Select(hash => frameAnalysis.Shaders.TryGetValue(hash, out var shader) ? shader : null).ExceptNull()
            .ForEach(s => s.Contexts.Select(c => resourceSelector(c)?.FirstOrDefault(r => r.Index == reg.Thing.Index)?.Asset).Distinct().ExceptNull()
            .ForEach(a =>
            {
                if (!a.VariableNames.Contains(reg.Thing))
                    a.VariableNames.Add(reg.Thing);
            })));
        }
    }
}
