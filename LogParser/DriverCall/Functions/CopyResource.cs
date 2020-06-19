namespace Migoto.Log.Parser.DriverCall
{
    public class CopyResource : Base
    {
        public CopyResource(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pDstResource { get; set; }

        public ulong pSrcResource { get; set; }

        public Slot.Resource Src { get; set; }

        public Slot.Resource Dst { get; set; }
    }
}
