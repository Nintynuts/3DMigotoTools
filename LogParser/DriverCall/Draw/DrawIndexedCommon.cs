namespace Migoto.Log.Parser.DriverCall.Draw
{
    public abstract class DrawIndexedCommon : Base
    {
        protected DrawIndexedCommon(uint order, DrawCall owner) : base(order, owner) { }

        public uint StartIndexLocation { get; set; }
        public uint BaseVertexLocation { get; set; }

        public uint? StartVertex => BaseVertexLocation;
        public uint? VertexCount => null;
        public uint? StartIndex => StartIndexLocation;
    }
}
