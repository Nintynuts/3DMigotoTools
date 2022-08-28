namespace Migoto.Log.Parser.ApiCalls;

public class ClearUnorderedAccessViewUint : Clear
{
    public ClearUnorderedAccessViewUint(uint order) : base(order) { }
    public ulong pUnorderedAccessView { get; set; }
    public ulong Values { get; set; }
}
