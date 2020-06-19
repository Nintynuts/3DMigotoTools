namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class Draw : Base, IDraw
    {
        public Draw(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint VertexCount { get; set; }

        public uint StartVertexLocation { get; set; }

        public uint? StartVertex => StartVertexLocation;
        public uint? EndVertex => StartVertexLocation + VertexCount;
        public uint? StartIndex => null;
        public uint? EndIndex => null;
        public uint? StartInstance => null;
        public uint? EndInstance => null;
    }
}
