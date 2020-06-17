using System.Collections.Generic;
using System.IO;
using System.Linq;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = new StreamReader(args[0]);

            var parser = new Parser(file);

            var frames = parser.Parse();

            var output = new StreamWriter(args[0].Replace(".txt", ".csv"));

            output.WriteLine($"Frame,Draw,{Slots("ib", 1).ToCSV()},{Slots("vb", 2).ToCSV()},VS,{Slots("cb", 14).ToCSV()}," +/*$"{Slots("t").ToCSV()},"+*/$"PS,{Slots("cb", 14).ToCSV()},{Slots("t").ToCSV()},{Slots("rt", 4).ToCSV()},rtD");

            frames.ForEach(frame =>
            {
                frame.DrawCalls.ForEach(drawCall =>
                {
                    var IndexBuffers = drawCall.SetIndexBuffer.Where(ib => ib.Buffer != null).Select(ib => new Resource(null) { Index = (int)ib.Offset, Asset = ib.Buffer }).ToList();
                    var columns = new string[][] {
                        new[] { $"{frame.Index}", $"{drawCall.Index:000000}" },
                        IndexBuffers.ToStrings(1),
                        drawCall.SetVertexBuffers.SelectMany(vb => vb.VertexBuffers).ToStrings(2),
                        // Vertex Shader
                        new[]{ $"{drawCall.VertexShader.SetShader.Shader.Hash:X}" },
                        drawCall.VertexShader.SetConstantBuffers.ConstantBuffers.ToStrings(14),
                        //drawCall.VertexShader.SetShaderResources.ResourceViews.ToStrings(),
                        // Pixel Shader
                        new[]{ $"{drawCall.PixelShader.SetShader.Shader.Hash:X}" },
                        drawCall.PixelShader.SetConstantBuffers.ConstantBuffers.ToStrings(14),
                        drawCall.PixelShader.SetShaderResources.ResourceViews.ToStrings(),
                        // Render Targets
                        drawCall.SetRenderTargets.RenderTargets.ToStrings(4),
                        new[]{ drawCall.SetRenderTargets.DepthStencil == null? string.Empty : $"{drawCall.SetRenderTargets.DepthStencil.Asset.Hash:X}" },
                    };
                    output.Write(columns.SelectMany(s => s).ToCSV());
                    output.WriteLine($",\"{drawCall.Logic}\"");
                });
            });
            output.Close();
        }

        private static IEnumerable<string> Slots(string type, int count = 16) => Enumerable.Range(0, count).Select(i => $"{type}{i}");
    }

    static class ConverterExtensions
    {
        public static string[] ToStrings<TResource>(this IEnumerable<TResource> items, int count = 16)
            where TResource : Resource
            => Enumerable.Range(0, count).Select(i => $"{items?.FirstOrDefault(r => r.Index == i)?.Asset.Hash:X}").ToArray();

        public static string ToCSV(this IEnumerable<string> items) => items.Aggregate((a, b) => $"{a},{b}");
    }
}
