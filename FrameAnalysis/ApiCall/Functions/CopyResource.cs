namespace Migoto.Log.Parser.ApiCalls;

public class CopyResource : ApiCall
{
    public CopyResource(uint order) : base(order) { }

    public ulong pDstResource { get; set; }
    public ulong pSrcResource { get; set; }

    public Slots.Resource? Src { get; set; }
    public Slots.Resource? Dst { get; set; }
}
