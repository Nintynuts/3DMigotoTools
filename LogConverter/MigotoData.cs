using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Config;
    using Parser;
    using Parser.ApiCalls;
    using Parser.Slots;
    using ShaderFixes;

    public class MigotoData
    {
        public const string D3DX = "d3dx.ini";
        public DrawCallColumns columns;
        public List<(ShaderType type, ShaderColumns columns)> shaderColumns;
        public FrameAnalysis frameAnalysis;
        public readonly Config config = new Config();
        public readonly ShaderFixes shaderFixes = new ShaderFixes();
        public bool metadataLoaded;

        public bool LoadLog(string validFilePath, Action<string> logger)
        {
            StreamReader frameAnalysisFile = null;
            while (frameAnalysisFile == null)
                try { frameAnalysisFile = new StreamReader(validFilePath); } catch { }

            frameAnalysis = new FrameAnalysis(frameAnalysisFile, logger);

            if (!frameAnalysis.Parse())
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
            columns = DrawCallColumns.Index;
            shaderColumns = new List<(ShaderType type, ShaderColumns columns)>();
            if (cmdColumns?.Any() == true)
            {
                columnSelection = cmdColumns;
            }
            else
            {
                if (!ConsoleEx.ReadLineOrEsc("Please enter column selection (default: VB IB VS PS OM Logic): ", out var result))
                {
                    Console.WriteLine("Export Log aborted");
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
                        shaderColumns.Add((ShaderTypes.FromLetter[tokens[0][0]], Enums.Parse<ShaderColumns>(tokens[1])));
                    else if (token.Last() == 'S')
                        shaderColumns.Add((ShaderTypes.FromLetter[token[0]], ShaderColumns.All));
                    else
                        columns |= Enums.Parse<DrawCallColumns>(column);
                }
                catch
                {
                    Console.WriteLine($"Failed to parse column: {column}");
                }
            }

            // Consolidate duplicate entries, just in case!
            shaderColumns = shaderColumns.GroupBy(s => s.type).Select(s => (s.Key, s.Select(c => c.columns).Aggregate((a, b) => a | b))).ToList();

            return true;
        }

        public void GetMetadata(string rootPath)
        {
            Console.Write("Reading metadata from ini config files...");
            config.Read(Path.Combine(rootPath, D3DX));
            ConsoleEx.ClearLine();
            Console.WriteLine($"TextureOverrides: {config.TextureOverrides.Count}\nShaderOverrides: {config.ShaderOverrides.Count}");
            Console.Write("Reading metadata from shader fixes...");
            shaderFixes.Scrape(rootPath);
            ConsoleEx.ClearLine();
            Console.WriteLine($"Shaders: {shaderFixes.ShaderNames.Count}\nTexture Registers: {shaderFixes.Textures.Count}\nConstant Buffer Registers {shaderFixes.ConstantBuffers.Count}");
            metadataLoaded = true;
        }

        public void LinkOverrides()
        {
            config.TextureOverrides.ForEach(to =>
            {
                if (frameAnalysis.Assets.TryGetValue(to.Hash, out var asset))
                    asset.Override = to;
            });
            config.ShaderOverrides.ForEach(so =>
            {
                if (frameAnalysis.Shaders.TryGetValue(so.Hash, out var shader))
                    shader.Override = so;
            });
        }

        public void LinkShaderFixes()
        {
            shaderFixes.ShaderNames.ForEach(sf =>
            {
                if (frameAnalysis.Shaders.TryGetValue(sf.Hash, out var shader))
                    shader.Fix = sf;
            });
            LinkRegisters(shaderFixes.Textures, s => s.SetShaderResources?.ResourceViews);
            LinkRegisters(shaderFixes.ConstantBuffers, s => s.SetConstantBuffers?.ConstantBuffers);
        }

        private void LinkRegisters(List<ShaderUsage<Register>> registers, Func<ShaderContext, IEnumerable<IResourceSlot>> resourceSelector)
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
