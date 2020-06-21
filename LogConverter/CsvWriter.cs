using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Migoto.Log.Parser;
using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Converter
{
    public static class CsvWriter
    {
        public static void Write(List<Frame> frames, StreamWriter output)
        {
            var buffers = new IColumns[] {
                new Column("Draw", dc => dc.Index),
                new Column("Topology", dc => dc.PrimitiveTopology?.Topology),
                new Column("vb#", dc => dc.Draw?.StartVertex.AsString()),
                new Column("vb*", dc => dc.Draw?.VertexCount.AsString()),
                new HashColumnSet("vb", dc => dc.SetVertexBuffers, IASetVertexBuffers.UsedSlots),
                new Column("ib#", dc => dc.Draw?.StartIndex.AsString()),
                new Column("ib*", dc => dc.Draw?.IndexCount.AsString()),
                new Column("ib^#", dc => dc.Draw?.StartInstance.AsString()),
                new Column("ib^*", dc => dc.Draw?.InstanceCount.AsString()),
                new HashColumn("ib", dc => dc.SetIndexBuffer?.Buffer),
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
            };

            var hashes = new[] { buffers, shaders, outputs }.SelectMany(c => c).ToList();

            output.WriteLine($"Frame,{hashes.SelectMany(c => c.Columns).ToCSV()},Pre,Post");

            var logicSplit = new Regex(@"(?<! )(?=post)");

            frames.ForEach(frame =>
            {
                frame.DrawCalls.ForEach(drawCall =>
                {
                    output.Write($"{frame.Index},{hashes.SelectMany(c => c.GetValues(drawCall)).ToCSV()}");
                    output.WriteLine($",\"{logicSplit.Replace(drawCall.Logic ?? "", "\",\"")}\"");
                });
            });
            output.Close();
        }

        private static string AsString(this uint? number) => number?.ToString() ?? "?";

        private static string ToCSV(this IEnumerable<string> items) => items.Aggregate((a, b) => $"{a},{b}");
    }
}
