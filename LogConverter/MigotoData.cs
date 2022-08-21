using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Config;

    using Parser;
    using Parser.ApiCalls;
    using Parser.Assets;
    using Parser.Slots;

    using ShaderFixes;

    public class MigotoData
    {
        public const string D3DX = "d3dx.ini";
        public DrawCallColumns ColumnGroups { get; private set; }
        public SplitFrames SplitFrames { get; private set; }
        public List<(ShaderType type, ShaderColumns columns, int[] indices)> ShaderColumns { get; } = new();
        public DirectoryInfo? RootFolder { get; private set; }
        public Config Config { get; } = new Config();
        public ShaderFixes ShaderFixes { get; } = new ShaderFixes();

        private readonly IUserInterface ui;

        internal MigotoData(IUserInterface ui) => this.ui = ui;

        public FrameAnalysis? LoadLog(FileInfo file, Action<string> logger)
        {
            ui.Event("Waiting for log to be readable...");
            using var frameAnalysisFile = file.TryOpenRead()!;
            ui.Event("Reading log...");
            var frameAnalysis = new FrameAnalysis(frameAnalysisFile, logger);

            if (!frameAnalysis.Parse())
            {
                logger("Provided file is not a 3DMigoto FrameAnalysis log file");
                return null;
            }
            if (!Config.Files.Any())
            {
                var possibleRoot = file.Directory?.Parent;
                if (possibleRoot?.GetFiles(D3DX).FirstOrDefault() is { } d3dx)
                    GetMetadata(d3dx);
            }
            LinkOverrides(frameAnalysis);
            LinkShaderFixes(frameAnalysis);

            return frameAnalysis;
        }

        public bool GetColumnSelection(IEnumerable<string>? cmdColumns = null)
        {
            ColumnGroups = DrawCallColumns.Index;

            string? initial = cmdColumns?.Any() == true ? cmdColumns.Delimit(' ') : null;

            return ui.GetValid("column selection (default: IA VS PS OM Logic)", initial, out initial, columnStr =>
            {
                var columnSelection = columnStr.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (columnSelection.Count == 0 || columnSelection.First() == string.Empty)
                    columnSelection.AddRange(new[] { "All", "VS", "PS" });

                var shaderColumns = new List<(ShaderType type, ShaderColumns columns, int[] indices)>();

                foreach (var column in columnSelection)
                {
                    try
                    {
                        var token = column.ToUpper();
                        var tokens = token.Split('-', ':');
                        if (tokens[0].Last() == 'S')
                        {
                            var shaderType = ShaderTypes.FromLetter[tokens[0][0]];
                            var columnType = tokens.Length > 1 ? Enums.Parse<ShaderColumns>(tokens[1]) : Converter.ShaderColumns.All;
                            var indices = tokens.Length > 2 ? tokens[2].Split(',').Select(int.Parse).ToArray() : Array.Empty<int>();
                            shaderColumns.Add((shaderType, columnType, indices));
                            continue;
                        }
                        ColumnGroups |= Enums.Parse<DrawCallColumns>(column);
                    }
                    catch
                    {
                        return (false, $"Failed to parse column: '{column}'", columnStr.Replace(column, "").Replace("  ", " "));
                    }
                }

                // Consolidate duplicate entries, just in case!
                ShaderColumns.Clear();
                ShaderColumns.AddRange(shaderColumns.GroupBy(s => s.type).Select(s => (s.Key, s.Select(c => c.columns).Aggregate((a, b) => a | b), s.SelectMany(c => c.indices).ToArray())));
                return (true, "", columnStr);
            });
        }

        public void GetMetadata(FileInfo d3dx)
        {
            RootFolder = d3dx.Directory!;
            ui.Status("Reading metadata from ini config files...");
            Config.Read(d3dx, reset: true);
            ui.Event($"TextureOverrides: {Config.TextureOverrides.Count()}\nShaderOverrides: {Config.ShaderOverrides.Count()}");

            if (Config.OverrideDirectory is null)
                return; // Unlikely to happen, as this is in the default d3dx.ini

            ui.Status("Reading metadata from shader fixes...");
            ShaderFixes.Scrape(RootFolder.SubDirectory(Config.OverrideDirectory));
            ui.Event($"Shaders: {ShaderFixes.ShaderNames.Count}\nTexture Registers: {ShaderFixes.Textures.Count}\nConstant Buffer Registers {ShaderFixes.ConstantBuffers.Count}");
        }

        public void GetSplitFile()
        {
            const string prompt = "whether you want to split frames into separate files (Yes/No [default]/Both)";

            if (ui.GetValid(prompt, SplitFrames.No, out var split, input => ParseSplitFrames(input) is { } split ? (true, "", split) : (false, "Please try again", SplitFrames.No)))
                SplitFrames = split;

            static SplitFrames? ParseSplitFrames(string input) => Enum.GetValues<SplitFrames>().Cast<SplitFrames?>().FirstOrDefault(v => v.ToString()?.StartsWith(input, StringComparison.OrdinalIgnoreCase) == true);
        }

        public void LinkOverrides(FrameAnalysis frameAnalysis)
        {
            LinkOverrides(Config.TextureOverrides, frameAnalysis.Assets);
            LinkOverrides(Config.ShaderOverrides, frameAnalysis.Shaders);
        }

        private static void LinkOverrides<THash, TAsset>(IEnumerable<Override<THash>> overrides, Dictionary<THash, TAsset> assets)
            where THash : struct where TAsset : IConfigOverride<THash>
        {
            overrides.GroupBy(to => to.Hash).ForEach(o =>
            {
                if (assets.TryGetValue(o.Key, out var asset))
                    asset.Override = o.Count() == 1 ? o.Single() : new MultiOverride<THash>(o);
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
