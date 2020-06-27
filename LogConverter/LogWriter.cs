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

    using Column = Column<Parser.DrawCall, object>;
    using IColumns = IColumns<Parser.DrawCall>;

    public enum DrawCallColumns
    {
        Index = 0,
        VB = 1,
        IB = 2,
        OM = 16,
        Logic = 32,
        All = VB|IB|OM|Logic,
    }

    public enum ShaderColumns
    {
        Hash = 0,
        CB = 1,
        T = 2,
        All = Hash|CB|T,
    }

    public static class LogWriter
    {
        private class AssetColumnSet : ColumnSet<DrawCall, IResource>
        {
            public AssetColumnSet(string name, Func<DrawCall, IMultiSlot> provider, IEnumerable<int> columns)
                : base(name, dc => provider(dc)?.Slots, GetValue, columns) { }

            public static string GetValue(DrawCall ctx, IResource item)
            {
                return item == null ? string.Empty
                    : item.Asset == null ? "No Hash"
                    : item.Asset.Hex + (GetModifier(ctx, item).Any(m => m.Slot?.Asset == item.Asset) ? "*" : string.Empty);
            }

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

        private class HashColumn : Column<DrawCall, IHash>
        {
            public HashColumn(string name, Func<DrawCall, IHash> provider)
                : base(name, provider, GetValue) { }

            public static string GetValue(DrawCall dc, IHash hash)
                => hash?.Hex ?? string.Empty;
        }

        private class UintColumn : Column<DrawCall, uint?>
        {
            public UintColumn(string name, Func<DrawCall, uint?> provider)
                : base(name, provider, AsString) { }

            private static string AsString(DrawCall dc, uint? number)
                => number?.ToString() ?? "?";
        }

        public static void Write(List<Frame> frames, StreamWriter output, DrawCallColumns columnGroups, IEnumerable<(ShaderType type, ShaderColumns columns)> shaders)
        {
            var logicSplit = new Regex(@"(?<! )(?=post)");

            var columns = new List<IColumns>();

            if (frames.Count > 1)
                columns.Add(new Column("Frame", dc => dc.Owner.Index));

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
                if (subColumns.HasFlag(ShaderColumns.Hash))
                    yield return new HashColumn($"{x}s", dc => (dc.Shader(shaderType).SetShader?.Shader));
                if (subColumns.HasFlag(ShaderColumns.CB))
                    yield return new AssetColumnSet($"{x}s-cb", dc => dc.Shader(shaderType).SetConstantBuffers, SetConstantBuffers.UsedSlots.GetOrAdd(shaderType));
                if (subColumns.HasFlag(ShaderColumns.T))
                    yield return new AssetColumnSet($"{x}s-t", dc => dc.Shader(shaderType).SetShaderResources, SetShaderResources.UsedSlots.GetOrAdd(shaderType));
            }

            columns.AddRange(shaders.OrderBy(s => s.type).SelectMany(GetShaderColumns));

            if (columnGroups.HasFlag(DrawCallColumns.OM))
                columns.AddRange(new IColumns[] {
                    new AssetColumnSet("o", dc => dc.SetRenderTargets, OMSetRenderTargets.UsedSlots),
                    new HashColumn("oD", dc => dc.SetRenderTargets?.DepthStencil?.Asset),
                });

            if (columnGroups.HasFlag(DrawCallColumns.Logic))
                columns.Add(new Column("Pre,Post", dc => $"\"{logicSplit.Replace(dc.Logic ?? "", "\",\"")}\""));

            output.WriteLine($"{columns.Headers()}");
            frames.ForEach(frame => frame.DrawCalls.ForEach(drawCall => output.WriteLine($"{columns.Values(drawCall)}")));
            output.Close();
        }
    }
}
