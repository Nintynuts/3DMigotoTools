namespace Migoto.Log.Parser.DriverCall
{
    public class ClearUnorderedAccessViewUint : Base
    {
        public ClearUnorderedAccessViewUint(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pUnorderedAccessView { get; set; }
        public ulong Values { get; set; }
    }
}
