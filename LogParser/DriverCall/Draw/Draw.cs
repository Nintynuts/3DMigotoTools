namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class Draw : Base, IDraw
    {
        public Draw(uint order) : base(order) { }

        public uint StartVertexLocation { get; set; }

        public uint? StartVertex => StartVertexLocation;
        public uint? VertexCount { get; set; }
        public uint? StartIndex => null;
        public uint? IndexCount => null;
        public uint? StartInstance => null;
        public uint? InstanceCount => null;
    }
}
