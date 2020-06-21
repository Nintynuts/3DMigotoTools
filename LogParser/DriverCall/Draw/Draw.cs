namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class Draw : Base, IDraw
    {
        public Draw(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint? VertexCount { get; set; }

        public uint StartVertexLocation { get; set; }

        public uint? StartVertex => StartVertexLocation;
        public uint? StartIndex => null;
        public uint? IndexCount => null;
        public uint? StartInstance => null;
        public uint? InstanceCount => null;
    }
}
