namespace Migoto.Log.Parser.DriverCall
{
    public class ClearUnorderedAccessViewUint : ClearBase
    {
        public ClearUnorderedAccessViewUint(uint order) : base(order) { }
        public ulong pUnorderedAccessView { get; set; }
        public ulong Values { get; set; }
    }
}
