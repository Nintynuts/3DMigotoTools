using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Migoto.Log.Parser;
using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Converter
{
    using Buffer = Parser.Asset.Buffer;
    using Column = Column<DrawCall, object>;
    using IColumns = IColumns<DrawCall>;

    public static class LogWriter
    {
        private class AssetColumnSet : ColumnSet<DrawCall, IResource>
        {
            public AssetColumnSet(string name, Func<DrawCall, IResourceSlots> provider, IEnumerable<int> columns)
                : base(name, dc => provider(dc)?.AllSlots, GetValue, columns) { }

            public static string GetValue(DrawCall ctx, IResource item)
            {
                return item == null ? string.Empty
                    : item.Asset == null ? "No Hash"
                    : item.Asset.Hex + (GetModifier(ctx, item).Any(m => m.Target?.Asset == item.Asset) ? "*" : string.Empty);
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

        public static void Write(List<Frame> frames, StreamWriter output)
        {
            var logicSplit = new Regex(@"(?<! )(?=post)");

            var buffers = new IColumns[] {
                new Column("Draw", dc => dc.Index),
                new Column("Topology", dc => dc.PrimitiveTopology?.Topology),
                new UintColumn("vb_", dc => dc.Draw?.StartVertex),
                new UintColumn("vb#", dc => dc.Draw?.VertexCount),
                new AssetColumnSet("vb", dc => dc.SetVertexBuffers, IASetVertexBuffers.UsedSlots),
                new UintColumn("ib_", dc => dc.Draw?.StartIndex),
                new UintColumn("ib#", dc => dc.Draw?.IndexCount),
                new HashColumn("ib", dc => dc.SetIndexBuffer?.Buffer),
                new UintColumn("inst_", dc => dc.Draw?.StartInstance),
                new UintColumn("inst#", dc => dc.Draw?.InstanceCount),
            };

            IColumns[] ShaderColumns(ShaderType shaderType)
            {
                char x = shaderType.ToString().ToLower()[0];
                return new IColumns[]
                {
                    new HashColumn($"{x}s", dc => (dc.Shader(shaderType).SetShader?.Shader)),
                    new AssetColumnSet($"{x}s-cb", dc => dc.Shader(shaderType).SetConstantBuffers, SetConstantBuffers.UsedSlots.GetOrAdd(shaderType)),
                    new AssetColumnSet($"{x}s-t" , dc => dc.Shader(shaderType).SetShaderResources, SetShaderResources.UsedSlots.GetOrAdd(shaderType)),
                };
            }

            var shaders = new[] { ShaderType.Vertex, ShaderType.Pixel }.SelectMany(ShaderColumns);

            var outputs = new IColumns[] {
                new AssetColumnSet("o", dc => dc.SetRenderTargets, OMSetRenderTargets.UsedSlots),
                new HashColumn("oD", dc => dc.SetRenderTargets?.DepthStencil?.Asset),
                new Column("Pre,Post", dc => $"\"{logicSplit.Replace(dc.Logic ?? "", "\",\"")}\"" ),
            };

            var hashes = new[] { buffers, shaders, outputs }.SelectMany(c => c).ToList();

            output.WriteLine($"Frame,{hashes.Headers()}");

            frames.ForEach(frame =>
            {
                frame.DrawCalls.ForEach(drawCall =>
                {
                    output.WriteLine($"{frame.Index},{hashes.Values(drawCall)}");
                });
            });
            output.Close();
        }
    }
}
