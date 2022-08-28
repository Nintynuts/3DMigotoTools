namespace Migoto.Log.Parser.ApiCalls;

public class ClearDepthStencilView : Clear
{
    public ClearDepthStencilView(uint order) : base(order) { }
    public ulong pDepthStencilView { get; set; }
    public uint ClearFlags { get; set; }
    public float Depth { get; set; }
    public ulong Stencil { get; set; }
}
