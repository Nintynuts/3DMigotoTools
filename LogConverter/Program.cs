using System;
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
                    var VertexBuffers = drawCall.SetVertexBuffers.SelectMany(vb => vb.VertexBuffers);
                    var columns = new string[][] {
                        new[] { $"{frame.Index}", $"{drawCall.Index:000000}" },
                        (null as DriverCall.Base).ToStrings(_ => IndexBuffers,1),
                        (null as DriverCall.Base).ToStrings(_ => VertexBuffers,2),
                        // Vertex Shader
                        new[]{ $"{drawCall.VertexShader.SetShader.Shader.Hash:X}" },
                        drawCall.VertexShader.SetConstantBuffers.ToStrings(dc => dc?.ConstantBuffers,14),
                        //drawCall.VertexShader.SetShaderResources.ToStrings(dc => dc?.ResourceViews),
                        // Pixel Shader
                        new[]{ $"{drawCall.PixelShader.SetShader.Shader.Hash:X}" },
                        drawCall.PixelShader.SetConstantBuffers.ToStrings(dc => dc?.ConstantBuffers,14),
                        drawCall.PixelShader.SetShaderResources.ToStrings(dc => dc?.ResourceViews),
                        // Render Targets
                        drawCall.SetRenderTargets.ToStrings(dc => dc?.RenderTargets, 4),
                        new[]{ drawCall.SetRenderTargets.DepthStencil == null? string.Empty : $"D:{drawCall.SetRenderTargets.DepthStencil.Asset.Hash:X}" },
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
        public static string[] ToStrings<TDriverCall, TResource>(this TDriverCall driverCall, Func<TDriverCall, IEnumerable<TResource>> items, int count = 16)
            where TDriverCall : DriverCall.Base
            where TResource : Resource
            => Enumerable.Range(0, count).Select(i => $"{items(driverCall)?.FirstOrDefault(r => r.Index == i)?.Asset.Hash:X}").ToArray();

        public static string ToCSV(this IEnumerable<string> items) => items.Aggregate((a, b) => $"{a},{b}");
    }
}
