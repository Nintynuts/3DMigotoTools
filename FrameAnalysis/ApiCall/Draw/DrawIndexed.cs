namespace Migoto.Log.Parser.ApiCalls.Draw
{
    public class DrawIndexed : ApiCall, IDrawIndexed
    {
        public DrawIndexed(uint order) : base(order) { }

        public uint StartIndexLocation { get; set; }
        public uint BaseVertexLocation { get; set; }

        public uint? StartVertex => BaseVertexLocation;
        public uint? VertexCount => null;
        public uint? StartIndex => StartIndexLocation;
        public uint? IndexCount { get; set; }
        public uint? StartInstance => null;
        public uint? InstanceCount => null;
    }
}
