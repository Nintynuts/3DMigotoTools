namespace Migoto.Log.Parser.ApiCalls.Draw;

public class DrawIndexedInstanced : ApiCall, IDrawInstanced, IDrawIndexed
{
    public DrawIndexedInstanced(uint order) : base(order) { }

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
