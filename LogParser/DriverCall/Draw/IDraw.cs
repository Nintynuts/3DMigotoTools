namespace Migoto.Log.Parser.DriverCall.Draw
{
    public interface IDraw
    {
        uint? StartVertex { get; }
        uint? VertexCount { get; }
        uint? StartIndex { get; }
        uint? IndexCount { get; }
        uint? StartInstance { get; }
        uint? InstanceCount { get; }
    }

    public interface IDrawInstanced
    {
        public uint StartInstanceLocation { get; set; }
    }

    public interface IDrawIndexed
    {
        public uint StartIndexLocation { get; set; }
        public uint BaseVertexLocation { get; set; }
    }
}
