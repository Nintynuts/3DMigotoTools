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
        public List<(ShaderType type, ShaderColumns columns)> ShaderColumns { get; private set; }
        public FrameAnalysis FrameAnalysis { get; private set; }
        public Config Config { get; } = new Config();
        public ShaderFixes ShaderFixes { get; } = new ShaderFixes();

        private readonly IUserInterface ui;
        private bool metadataLoaded;

        internal MigotoData(IUserInterface ui) => this.ui = ui;

        public bool LoadLog(string validFilePath, Action<string> logger)
        {
            using var frameAnalysisFile = IOHelpers.TryReadFile(validFilePath);

            FrameAnalysis = new FrameAnalysis(frameAnalysisFile, logger);

            if (!FrameAnalysis.Parse())
            {
                logger("Provided file is not a 3DMigoto FrameAnalysis log file");
                return false;
            }
            if (!metadataLoaded)
            {
                DirectoryInfo possibleRoot = Directory.GetParent(validFilePath).Parent;
                if (possibleRoot.GetFiles(D3DX).Any())
                    GetMetadata(possibleRoot.FullName);
            }
            LinkOverrides();
            LinkShaderFixes();

            return true;
        }

        public bool GetColumnSelection(IEnumerable<string> cmdColumns = null)
        {
            IEnumerable<string> columnSelection;
            ColumnGroups = DrawCallColumns.Index;
            ShaderColumns = new List<(ShaderType type, ShaderColumns columns)>();
            if (cmdColumns?.Any() == true)
            {
                columnSelection = cmdColumns;
            }
            else
            {
                if (!ui.GetInfo("column selection (default: VB IB VS PS OM Logic)", out var result))
                {
                    ui.Event("Export Log aborted");
                    return false;
                }
                columnSelection = result.Split(' ');
            }

            if (columnSelection.Count() == 1 && columnSelection.First() == string.Empty)
                columnSelection = new[] { "All", "VS", "PS" };

            foreach (var column in columnSelection)
            {
                try
                {
                    var token = column.ToUpper();
                    var tokens = token.Split('-');
                    if (tokens.Length > 1)
                        ShaderColumns.Add((ShaderTypes.FromLetter[tokens[0][0]], Enums.Parse<ShaderColumns>(tokens[1])));
                    else if (token.Last() == 'S')
                        ShaderColumns.Add((ShaderTypes.FromLetter[token[0]], Converter.ShaderColumns.All));
                    else
                        ColumnGroups |= Enums.Parse<DrawCallColumns>(column);
                }
                catch
                {
                    ui.Event($"Failed to parse column: {column}");
                }
            }

            // Consolidate duplicate entries, just in case!
            ShaderColumns = ShaderColumns.GroupBy(s => s.type).Select(s => (s.Key, s.Select(c => c.columns).Aggregate((a, b) => a | b))).ToList();

            return true;
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

        public void LinkOverrides()
        {
            Config.TextureOverrides.ForEach(to =>
            {
                if (FrameAnalysis.Assets.TryGetValue(to.Hash, out var asset))
                    asset.Override = to;
            });
            Config.ShaderOverrides.ForEach(so =>
            {
                if (FrameAnalysis.Shaders.TryGetValue(so.Hash, out var shader))
                    shader.Override = so;
            });
        }

        public void LinkShaderFixes()
        {
            ShaderFixes.ShaderNames.ForEach(sf =>
            {
                if (FrameAnalysis.Shaders.TryGetValue(sf.Hash, out var shader))
                    shader.Fix = sf;
            });
            LinkRegisters(ShaderFixes.Textures, s => s.SetShaderResources?.ResourceViews);
            LinkRegisters(ShaderFixes.ConstantBuffers, s => s.SetConstantBuffers?.ConstantBuffers);
        }

        private void LinkRegisters(List<ShaderUsage<Register>> registers, Func<ShaderContext, IEnumerable<IResourceSlot>> resourceSelector)
        {
            registers
            .ForEach(reg => reg.Hashes.Select(hash => FrameAnalysis.Shaders.TryGetValue(hash, out var shader) ? shader : null).ExceptNull()
            .ForEach(s => s.Contexts.Select(c => resourceSelector(c)?.FirstOrDefault(r => r.Index == reg.Thing.Index)?.Asset).Distinct().ExceptNull()
            .ForEach(a =>
            {
                if (!a.VariableNames.Contains(reg.Thing))
                    a.VariableNames.Add(reg.Thing);
            })));
        }
    }
}
