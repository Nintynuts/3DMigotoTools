namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class DrawIndexedInstanced : Base, IDraw, IDrawInstanced, IDrawIndexed
    {
        public DrawIndexedInstanced(uint order, DrawCall owner) : base(order, owner) { }

        public uint BaseVertexLocation { get; set; }
        public uint StartIndexLocation { get; set; }
        public uint IndexCountPerInstance { get; set; }
        public uint StartInstanceLocation { get; set; }

        public uint? StartVertex => BaseVertexLocation;
        public uint? VertexCount => null;
        public uint? StartIndex => StartIndexLocation;
        public uint? IndexCount => IndexCountPerInstance * InstanceCount;
        public uint? StartInstance => StartInstanceLocation;
        public uint? InstanceCount { get; set; }
    }
}
