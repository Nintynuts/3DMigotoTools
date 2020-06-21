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
}
