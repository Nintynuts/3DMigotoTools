using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Migoto.Log.Parser;
using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Converter
{
    using Column = Column<DrawCall>;
    using HashColumn = HashColumn<DrawCall>;
    using HashColumnSet = HashColumnSet<DrawCall>;
    using IColumns = IColumns<DrawCall>;

    public static class LogWriter
    {
        public static void Write(List<Frame> frames, StreamWriter output)
        {
            var logicSplit = new Regex(@"(?<! )(?=post)");

            var buffers = new IColumns[] {
                new Column("Draw", dc => dc.Index),
                new Column("Topology", dc => dc.PrimitiveTopology?.Topology),
                new Column("vb#", dc => dc.Draw?.StartVertex.AsString()),
                new Column("vb*", dc => dc.Draw?.VertexCount.AsString()),
                new HashColumnSet("vb", dc => dc.SetVertexBuffers, IASetVertexBuffers.UsedSlots),
                new Column("ib#", dc => dc.Draw?.StartIndex.AsString()),
                new Column("ib*", dc => dc.Draw?.IndexCount.AsString()),
                new HashColumn("ib", dc => dc.SetIndexBuffer?.Buffer),
                new Column("inst#", dc => dc.Draw?.StartInstance.AsString()),
                new Column("inst*", dc => dc.Draw?.InstanceCount.AsString()),
            };

            IColumns[] ShaderColumns(ShaderType shaderType)
            {
                char x = shaderType.ToString().ToLower()[0];
                return new IColumns[]
                {
                    new HashColumn($"{x}s", dc => (dc.Shader(shaderType).SetShader?.Shader)),
                    new HashColumnSet($"{x}s-cb", dc => dc.Shader(shaderType).SetConstantBuffers, SetConstantBuffers.UsedSlots.GetOrAdd(shaderType)),
                    new HashColumnSet($"{x}s-t" , dc => dc.Shader(shaderType).SetShaderResources, SetShaderResources.UsedSlots.GetOrAdd(shaderType)),
                };
            }

            var shaders = new[] { ShaderType.Vertex, ShaderType.Pixel }.SelectMany(ShaderColumns);

            var outputs = new IColumns[] {
                new HashColumnSet("o", dc => dc.SetRenderTargets, OMSetRenderTargets.UsedSlots),
                new HashColumn("oD", dc => dc.SetRenderTargets?.DepthStencil?.Asset ),
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

        private static string AsString(this uint? number) => number?.ToString() ?? "?";
    }
}
