namespace Migoto.Log.Parser.ApiCalls.Draw;

public interface IDraw
{
    uint? StartVertex { get; }
    uint? VertexCount { get; }
    uint? StartIndex { get; }
    uint? IndexCount { get; }
    uint? StartInstance { get; }
    uint? InstanceCount { get; }
}

public interface IDrawInstanced : IDraw
{
    public uint StartInstanceLocation { get; set; }
}

public interface IDrawIndexed : IDraw
{
    public uint StartIndexLocation { get; set; }
    public uint BaseVertexLocation { get; set; }
}
