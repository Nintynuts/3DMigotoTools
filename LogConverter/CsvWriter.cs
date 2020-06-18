using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Migoto.Log.Parser;
using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Converter
{
    public static class CsvWriter
    {
        private class HashColumnSet
        {
            private readonly string name;

            private readonly Func<DrawCall, object> columns;

            private readonly int max;

            public HashColumnSet(string name, Func<DrawCall, object> columns, IEnumerable<Frame> frames)
            {
                this.name = name;
                this.columns = columns;
                max = frames.SelectMany(f => f.DrawCalls).Max(dc => GetIndex(columns(dc)));
            }

            private int GetIndex(object v)
            {
                if (v is IEnumerable<IResource> resources && resources.Any())
                    return resources.Max(r => r.Index);
                return -1;
            }

            public IEnumerable<string> Slots()
            {
                return max == -1 ? new[] { name } : Enumerable.Range(0, max + 1).Select(i => $"{name}{i}");
            }

            public IEnumerable<string> GetHashes(DrawCall dc)
            {
                var item = columns(dc);
                if (item == null)
                    return new[] { string.Empty };
                if (item is IEnumerable<IResource> resources && max != -1)
                    return Enumerable.Range(0, max + 1).Select(i => $"{resources.FirstOrDefault(r => r.Index == i)?.Asset.Hash:X}");
                if (item is Base asset)
                    return new[] { $"{asset.Hash:X}" };
                if (item is Shader shader)
                    return new[] { $"{shader.Hash:X}" };

                throw new ArgumentException("Unexpected type, cannot generate a hash!");
            }
        }

        public static void Write(List<Frame> frames, string fileName)
        {
            var output = new StreamWriter(fileName);

            var columns = new HashColumnSet[] {
                new HashColumnSet("ib", dc => dc.SetIndexBuffer.Where(ib => ib.Buffer != null), frames),
                new HashColumnSet("vb", dc => dc.SetVertexBuffers.SelectMany(vb => vb.VertexBuffers), frames),
                // Vertex Shader
                new HashColumnSet("vs", dc => dc.VertexShader.SetShader?.Shader, frames),
                new HashColumnSet("cb", dc => dc.VertexShader.SetConstantBuffers?.ConstantBuffers, frames),
                new HashColumnSet("t" , dc => dc.VertexShader.SetShaderResources?.ResourceViews, frames),
                // Pixel Shader
                new HashColumnSet("ps", dc => dc.PixelShader.SetShader?.Shader, frames),
                new HashColumnSet("cb", dc => dc.PixelShader.SetConstantBuffers?.ConstantBuffers, frames),
                new HashColumnSet("t" , dc => dc.PixelShader.SetShaderResources?.ResourceViews, frames),
                // Render Targets
                new HashColumnSet("o", dc => dc.SetRenderTargets?.RenderTargets, frames),
                new HashColumnSet("oD", dc => dc.SetRenderTargets?.DepthStencil?.Asset , frames),
            };

            output.WriteLine($"Frame,Draw,{columns.SelectMany(c => c.Slots()).ToCSV()},Pre,Post");

            var logicSplit = new Regex(@"(?<! )(?=post)");

            frames.ForEach(frame =>
            {
                frame.DrawCalls.ForEach(drawCall =>
                {
                    output.WriteLine($"{frame.Index},{drawCall.Index:000000},{columns.SelectMany(c => c.GetHashes(drawCall)).ToCSV()},\"{logicSplit.Replace(drawCall.Logic ?? "", "\",\"")}\"");
                });
            });
            output.Close();
        }

        private static string ToCSV(this IEnumerable<string> items) => items.Aggregate((a, b) => $"{a},{b}");
    }
}
