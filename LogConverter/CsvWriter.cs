using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Migoto.Log.Parser;
using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.DriverCall.Draw;
using Migoto.Log.Parser.Slot;


namespace Migoto.Log.Converter
{
    using Asset = Parser.Asset.Base;

    public static class CsvWriter
    {
        private class HashColumnSet
        {
            private readonly string name;

            private readonly Func<DrawCall, object> items;

            private readonly int max;

            private IEnumerable<int> Columns => Enumerable.Range(0, max);

            public HashColumnSet(string name, Func<DrawCall, object> items, IEnumerable<Frame> frames)
            {
                this.name = name;
                this.items = items;
                max = frames.SelectMany(f => f.DrawCalls).Max(dc => GetIndex(items(dc))) + 1;
            }

            private int GetIndex(object v)
            {
                if (v is IEnumerable<IResource> resources && resources.Any())
                    return resources.Max(r => r.Index);
                return 0;
            }

            public IEnumerable<string> Slots()
            {
                return max == 1 ? new[] { name } : Columns.Select(i => $"{name}{i}");
            }

            public IEnumerable<string> GetHashes(DrawCall dc)
            {
                var item = items(dc);
                if (item == null)
                    return Columns.Select(i => string.Empty);
                if (item is IEnumerable<IResource> resources)
                    return Columns.Select(i => $"{resources.FirstOrDefault(r => r.Index == i)?.Asset.Hash:X}");
                if (item is Asset asset)
                    return new[] { $"{asset.Hash:X}" };
                if (item is Shader shader)
                    return new[] { $"{shader.Hash:X}" };

                throw new ArgumentException("Unexpected type, cannot generate a hash!");
            }
        }

        public static void Write(List<Frame> frames, string fileName)
        {
            var output = new StreamWriter(fileName);

            var buffers = new HashColumnSet[] {
                new HashColumnSet("vb", dc => dc.SetVertexBuffers.SelectMany(vb => vb.VertexBuffers), frames),
                new HashColumnSet("ib", dc => dc.SetIndexBuffer.Where(ib => ib.Buffer != null), frames),
            };

            HashColumnSet[] ShaderColumns(ShaderType shaderType)
            {
                char x = shaderType.ToString().ToLower()[0];
                return new[]
                {
                    new HashColumnSet($"{x}s", dc => (dc.Shader(shaderType).SetShader?.Shader), frames),
                    new HashColumnSet($"{x}s-cb", dc => (dc.Shader(shaderType).SetConstantBuffers?.ConstantBuffers), frames),
                    new HashColumnSet($"{x}s-t" , dc => (dc.Shader(shaderType).SetShaderResources?.ResourceViews), frames),
                };
            }

            var shaders = new[] { ShaderType.Vertex, ShaderType.Pixel }.SelectMany(ShaderColumns);

            var outputs = new HashColumnSet[] {
                new HashColumnSet("o", dc => dc.SetRenderTargets?.RenderTargets, frames),
                new HashColumnSet("oD", dc => dc.SetRenderTargets?.DepthStencil?.Asset , frames),
            };

            var hashes = new[] { buffers, shaders, outputs }.SelectMany(c => c).ToList();

            output.WriteLine($"Frame,Draw,Topology,Vertices,Indices,Instances,{hashes.SelectMany(c => c.Slots()).ToCSV()},Pre,Post");

            var logicSplit = new Regex(@"(?<! )(?=post)");

            frames.ForEach(frame =>
            {
                frame.DrawCalls.ForEach(drawCall =>
                {
                    output.Write($"{frame.Index},{drawCall.Index},{drawCall.PrimitiveTopology?.Topology},{GetDrawData(drawCall.Draw)},");
                    output.Write(hashes.SelectMany(c => c.GetHashes(drawCall)).ToCSV());
                    output.WriteLine($",\"{logicSplit.Replace(drawCall.Logic ?? "", "\",\"")}\"");
                });
            });
            output.Close();
        }

        private static string GetDrawData(IDraw draw)
        {
            return $"{draw?.StartVertex.AsString()}-{draw?.EndVertex.AsString()},{draw?.StartIndex.AsString()}-{draw?.EndIndex.AsString()},{draw?.StartInstance.AsString()}-{draw?.EndInstance.AsString()}";
        }

        private static string AsString(this uint? number) => number?.ToString() ?? "?";

        private static string ToCSV(this IEnumerable<string> items) => items.Aggregate((a, b) => $"{a},{b}");
    }
}
