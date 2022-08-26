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
    public enum DrawCallColumnGroups
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

    public enum SplitFrames
    {
        No,
        Yes,
        Both,
    }


    public class LogWriter
    {
        private readonly List<Frame> frames;
        private readonly List<IColumns<DrawCall>> columns;
        private readonly Func<string, StreamWriter?> outputSrc;
        private readonly SplitFrames splitFrames;

        private class AssetColumnSet : ColumnSet<DrawCall, IResource>
        {
            public AssetColumnSet(string name, Func<DrawCall, IMultiSlot<IResourceSlot>?> provider, IEnumerable<int> columns)
                : base(name.ToLower(), dc => provider(dc)?.Slots, GetValue, columns) { }

            public static string GetValue(DrawCall ctx, IResource? item)
            {
                return $"\"{GetText(ctx, item).Trim()}\"";
            }

            private static string GetText(DrawCall ctx, IResource? item)
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
                : base(name.ToLower(), provider, GetValue) { }

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

        public LogWriter(MigotoData data, FrameAnalysis frameAnalysis, Func<string, StreamWriter?> outputSrc)
        {
            this.outputSrc = outputSrc;
            frames = frameAnalysis.Frames;
            splitFrames = data.SplitFrames;
            if (data.ColumnData == null)
                throw new InvalidDataException("Columns haven't been set");

            var columnGroups = data.ColumnData.Groups;
            var shaderColumns = data.ColumnData.ShaderColumns;

            var logicSplit = new Regex(@"(?<=[\r\n])(?=\bpost\b)");

            columns = new List<IColumns> { new Column("Draw", dc => dc.Index) };

            if (columnGroups.HasFlag(DrawCallColumnGroups.VB))
                columns.AddRange(new IColumns[] {
                    new UintColumn("vb_", dc => dc.Draw?.StartVertex),
                    new UintColumn("vb#", dc => dc.Draw?.VertexCount),
                    new AssetColumnSet("vb", dc => dc.SetVertexBuffers, IASetVertexBuffers.UsedSlots),
                });

            if (columnGroups.HasFlag(DrawCallColumnGroups.IB))
                columns.AddRange(new IColumns[] {
                    new UintColumn("ib_", dc => dc.Draw?.StartIndex),
                    new UintColumn("ib#", dc => dc.Draw?.IndexCount),
                    new HashColumn("ib", dc => dc.SetIndexBuffer?.Buffer),
                });

            if (columnGroups.HasFlag(DrawCallColumnGroups.VB) || columnGroups.HasFlag(DrawCallColumnGroups.IB))
                columns.AddRange(new IColumns[] {
                    new UintColumn("inst_", dc => dc.Draw?.StartInstance),
                    new UintColumn("inst#", dc => dc.Draw?.InstanceCount),
                    new Column("Topology", dc => dc.PrimitiveTopology?.Topology),
                });

            IEnumerable<IColumns> GetShaderColumns(OutputColumns.Shader column)
            {
                var (shaderType, subColumns, indices) = column;
                char type = shaderType.ToString()[0];
                yield return new ShaderColumn($"{type}S", dc => (dc.Shaders[shaderType].SetShader?.Shader));

                if (subColumns.HasFlag(ShaderColumns.CB))
                    yield return AssetColumn(ShaderColumns.CB, s => s.SetConstantBuffers, SetConstantBuffers.UsedSlots);

                if (subColumns.HasFlag(ShaderColumns.T))
                    yield return AssetColumn(ShaderColumns.T, s => s.SetShaderResources, SetShaderResources.UsedSlots);

                AssetColumnSet AssetColumn(ShaderColumns column, Func<ShaderContext, IMultiSlot<IResourceSlot>?> provider, Dictionary<ShaderType, List<int>> usedSlots)
                    => new($"{type}S-{column}", dc => provider(dc.Shaders[shaderType]), indices.Count > 0 ? indices : usedSlots.GetOrAdd(shaderType));
            }

            columns.AddRange(shaderColumns.OrderBy(s => s.Type).SelectMany(GetShaderColumns));

            if (columnGroups.HasFlag(DrawCallColumnGroups.RT))
                columns.Add(new AssetColumnSet("o", dc => dc.SetRenderTargets, OMSetRenderTargets.UsedSlots));

            if (columnGroups.HasFlag(DrawCallColumnGroups.D))
                columns.Add(new HashColumn("oD", dc => dc.SetRenderTargets?.DepthStencil?.Asset));

            if (columnGroups.HasFlag(DrawCallColumnGroups.Logic))
                columns.Add(new Column("Pre,Post", dc => $"\"{logicSplit.Replace(dc.Logic ?? "", "\",\"")}\""));
        }

        public void Write()
        {
            if (splitFrames is SplitFrames.Yes or SplitFrames.Both)
                Enumerable.Range(0, frames.Count).ForEach(WriteSingle);
            if (splitFrames is SplitFrames.No or SplitFrames.Both)
                WriteAll();
        }

        private void WriteAll()
        {
            using var output = outputSrc("");
            if (output == null)
                return;

            if (frames.Count > 1)
                columns.Insert(0, new Column("Frame", dc => dc.Owner?.Index));

            output.WriteLine($"{columns.Headers()}");
            frames.ForEach(frame => WriteFrame(output, frame));

            if (frames.Count > 1)
                columns.RemoveAt(0);
        }

        private void WriteSingle(int index)
        {
            using var output = outputSrc("-" + index);
            if (output == null)
                return;
            output.WriteLine($"{columns.Headers()}");
            WriteFrame(output, frames[index]);
        }

        private void WriteFrame(StreamWriter output, Frame frame)
            => frame.DrawCalls.ForEach(drawCall => output.WriteLine($"{columns.Values(drawCall)}"));
    }
}
