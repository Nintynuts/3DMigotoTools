namespace Migoto.Log.Parser.ApiCalls.Draw
{
    public class DrawInstanced : ApiCall, IDrawInstanced
    {
        public DrawInstanced(uint order) : base(order) { }

        public uint? StartVertexLocation { get; set; }
        public uint? VertexCountPerInstance { get; set; }
        public uint StartInstanceLocation { get; set; }

        public uint? StartVertex => StartVertexLocation;
        public uint? VertexCount => VertexCountPerInstance * InstanceCount;
        public uint? StartIndex => null;
        public uint? IndexCount => null;
        public uint? StartInstance => StartInstanceLocation;
        public uint? InstanceCount { get; set; }
    }
}
