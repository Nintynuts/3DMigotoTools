namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class DrawIndexed : Base, IDraw
    {
        public DrawIndexed(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint IndexCount { get; set; }
        public uint StartIndexLocation { get; set; }
        public uint BaseVertexLocation { get; set; }

        public uint? StartVertex => BaseVertexLocation;
        public uint? EndVertex => null;
        public uint? StartIndex => StartIndexLocation;
        public uint? EndIndex => StartIndexLocation + IndexCount;
        public virtual uint? StartInstance => null;
        public virtual uint? EndInstance => null;
    }
}
