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

    public record OutputColumns(DrawCallColumnGroups Groups, IReadOnlyList<OutputColumns.Shader> ShaderColumns)
    {
        public static OutputColumns? Default { get; } = Parse(new[] { "All", "VS", "PS" });

        public static OutputColumns? Parse(IEnumerable<string> columnSelection)
        {
            List<Shader> shaderColumns = new();

            var columnGroups = DrawCallColumnGroups.Index;

            foreach (var column in columnSelection)
            {
                try
                {
                    var token = column.ToUpper();
                    var tokens = token.Split('-', ':');
                    if (tokens[0].Last() == 'S')
                        shaderColumns.Add(Shader.Parse(tokens));
                    else
                        columnGroups |= Enums.Parse<DrawCallColumnGroups>(column);
                }
                catch
                {
                    throw new InvalidDataException($"Failed to parse column: '{column}'");
                }
            }

            // Consolidate duplicate entries, just in case!
            shaderColumns = shaderColumns.GroupBy(s => s.Type).Select(FromGrouping).ToList();

            return columnGroups == DrawCallColumnGroups.Index && !shaderColumns.Any() ? null : new(columnGroups, shaderColumns);

            static Shader FromGrouping(IGrouping<ShaderType, Shader> shader)
                => new(shader.Key, shader.Select(c => c.Columns).Aggregate((a, b) => a | b), shader.SelectMany(c => c.Indices).ToList());
        }

        public record Shader(ShaderType Type, ShaderColumns Columns, IReadOnlyList<int> Indices)
        {
            public static Shader Parse(string[] tokens)
            {
                var shaderType = ShaderTypes.FromLetter[tokens[0][0]];
                var columnType = tokens.Length > 1 ? Enums.Parse<ShaderColumns>(tokens[1]) : Converter.ShaderColumns.All;
                var indices = tokens.Length > 2 ? tokens[2].Split(',').Select(int.Parse).ToArray() : Array.Empty<int>();
                return new Shader(shaderType, columnType, indices);
            }
        }
    }

    public class MigotoData
    {
        public const string D3DX = "d3dx.ini";

        public OutputColumns? ColumnData { get; private set; }
        public DirectoryInfo? RootFolder { get; set; }
        public Config Config { get; } = new Config();
        public ShaderFixes ShaderFixes { get; } = new ShaderFixes();
        public SplitFrames SplitFrames { get; set; }

        public void SetColumns(OutputColumns? columnData = null)
            => ColumnData = columnData ?? OutputColumns.Default;

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
