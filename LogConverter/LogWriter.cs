using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Migoto.Log.Converter
{
    using Parser;
    using Parser.ApiCalls;
    using Parser.Assets;
    using Parser.Slots;

    using Column = Column<Parser.DrawCall, object?>;
    using IColumns = IColumns<Parser.DrawCall>;

    [Flags]
    public enum DrawCallColumns
    {
        Index = 0,
        IA = VB | IB,
        VB = 1,
        IB = 2,
        OM = RT | D,
        RT = 16,
        D = 32,
        Logic = 64,
        All = IA | OM | Logic,
    }

    [Flags]
    public enum ShaderColumns
    {
        Hash = 0,
        CB = 1,
        T = 2,
        All = Hash | CB | T,
    }

    public static class LogWriter
    {
        private class AssetColumnSet : ColumnSet<DrawCall, IResource>
        {
            public AssetColumnSet(string name, Func<DrawCall, IMultiSlot?> provider, IEnumerable<int> columns)
                : base(name, dc => provider(dc)?.Slots, GetValue, columns) { }

            public static string GetValue(DrawCall ctx, IResource item)
            {
                return $"\"{GetText(ctx, item).Trim()}\"";
            }

            private static string GetText(DrawCall ctx, IResource item)
                => item == null ? string.Empty
                   : item.Asset == null ? "No Hash"
                   : item.Asset.Hex + AsteriskIfModified(ctx, item) + " " + item.Asset.GetName(item.Owner, (item as ISlot)?.Index ?? -1);

            private static string AsteriskIfModified(DrawCall ctx, IResource item)
                => GetModifier(ctx, item).Any(m => m.Slot?.Asset == item.Asset) ? "*" : string.Empty;

            public static IEnumerable<ISingleSlot> GetModifier(DrawCall ctx, IResource item)
            {
                if (item.Asset is Buffer)
                {
                    return ctx.Mappings;
                }
                else if (item.Asset is Texture texture)
                {
                    if (texture.IsRenderTarget)
                        return ctx.RenderTargetCleared;
                    else if (texture.IsDepthStencil)
                        return ctx.DepthStencilCleared;
                }
                return Enumerable.Empty<ISingleSlot>();
            }
        }

        private class ShaderColumn : Column<DrawCall, Shader?>
        {
            public ShaderColumn(string name, Func<DrawCall, Shader?> provider)
                : base(name, provider, GetValue) { }

            public static string GetValue(DrawCall dc, Shader? shader)
                => $"\"{GetText(shader).Trim()}\"";

            private static string GetText(Shader? shader) => (shader?.Hex ?? string.Empty) + " " + shader?.Name;
        }

        private class HashColumn : Column<DrawCall, IHash?>
        {
            public HashColumn(string name, Func<DrawCall, IHash?> provider)
                : base(name, provider, GetValue) { }

            public static string GetValue(DrawCall dc, IHash? hash)
                => $"\"{GetText(hash).Trim()}\"";

            private static string GetText(IHash? hash) => hash?.Hex ?? string.Empty;
        }

        private class UintColumn : Column<DrawCall, uint?>
        {
            public UintColumn(string name, Func<DrawCall, uint?> provider)
                : base(name, provider, AsString) { }

            private static string AsString(DrawCall _, uint? number)
                => number?.ToString() ?? "?";
        }

        public static string GetOutputFileFrom(string inputFilePath)
        {
            var frameAnalysisPattern = new Regex(@"(?<=FrameAnalysis([-\d]+)[\\/])(\w+)(?=\.txt)");
            if (frameAnalysisPattern.IsMatch(inputFilePath))
                inputFilePath = frameAnalysisPattern.Replace(inputFilePath, "$2$1");
            return inputFilePath.Replace(".txt", ".csv");
        }

        public static void Write(MigotoData data, StreamWriter output)
        {
            if (data.FrameAnalysis == null)
                return;

            var frames = data.FrameAnalysis.Frames;
            var columnGroups = data.ColumnGroups;
            var shaderColumns = data.ShaderColumns;

            var logicSplit = new Regex(@"(?<! )(?=post)");

            var columns = new List<IColumns>();

            if (frames.Count > 1)
                columns.Add(new Column("Frame", dc => dc.Owner?.Index));

            columns.Add(new Column("Draw", dc => dc.Index));

            if (columnGroups.HasFlag(DrawCallColumns.VB))
                columns.AddRange(new IColumns[] {
                    new UintColumn("vb_", dc => dc.Draw?.StartVertex),
                    new UintColumn("vb#", dc => dc.Draw?.VertexCount),
                    new AssetColumnSet("vb", dc => dc.SetVertexBuffers, IASetVertexBuffers.UsedSlots),
                });

            if (columnGroups.HasFlag(DrawCallColumns.IB))
                columns.AddRange(new IColumns[] {
                    new UintColumn("ib_", dc => dc.Draw?.StartIndex),
                    new UintColumn("ib#", dc => dc.Draw?.IndexCount),
                    new HashColumn("ib", dc => dc.SetIndexBuffer?.Buffer),
                });

            if (columnGroups.HasFlag(DrawCallColumns.VB) || columnGroups.HasFlag(DrawCallColumns.IB))
                columns.AddRange(new IColumns[] {
                    new UintColumn("inst_", dc => dc.Draw?.StartInstance),
                    new UintColumn("inst#", dc => dc.Draw?.InstanceCount),
                    new Column("Topology", dc => dc.PrimitiveTopology?.Topology),
                });

            IEnumerable<IColumns> GetShaderColumns((ShaderType shaderType, ShaderColumns columns) _)
            {
                var shaderType = _.shaderType;
                var subColumns = _.columns;
                char x = shaderType.ToString().ToLower()[0];
                yield return new ShaderColumn($"{x}s", dc => (dc.Shader(shaderType).SetShader?.Shader));
                if (subColumns.HasFlag(ShaderColumns.CB))
                    yield return new AssetColumnSet($"{x}s-cb", dc => dc.Shader(shaderType).SetConstantBuffers, SetConstantBuffers.UsedSlots.GetOrAdd(shaderType));
                if (subColumns.HasFlag(ShaderColumns.T))
                    yield return new AssetColumnSet($"{x}s-t", dc => dc.Shader(shaderType).SetShaderResources, SetShaderResources.UsedSlots.GetOrAdd(shaderType));
            }

            columns.AddRange(shaderColumns.OrderBy(s => s.type).SelectMany(GetShaderColumns));

            if (columnGroups.HasFlag(DrawCallColumns.RT))
                columns.Add(new AssetColumnSet("o", dc => dc.SetRenderTargets, OMSetRenderTargets.UsedSlots));

            if (columnGroups.HasFlag(DrawCallColumns.D))
                columns.Add(new HashColumn("oD", dc => dc.SetRenderTargets?.DepthStencil?.Asset));

            if (columnGroups.HasFlag(DrawCallColumns.Logic))
                columns.Add(new Column("Pre,Post", dc => $"\"{logicSplit.Replace(dc.Logic ?? "", "\",\"")}\""));

            output.WriteLine($"{columns.Headers()}");
            frames.ForEach(frame => frame.DrawCalls.ForEach(drawCall => output.WriteLine($"{columns.Values(drawCall)}")));
        }
    }
}
