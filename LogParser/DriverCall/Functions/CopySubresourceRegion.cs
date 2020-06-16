namespace Migoto.Log.Parser.DriverCall
{
    public class CopySubresourceRegion : Base
    {
        public CopySubresourceRegion(DrawCall owner) : base(owner)
        {
        }

        public uint pDstResource { get; set; }

        public uint DstSubresource { get; set; }

        public uint DstX { get; set; }

        public uint DstY { get; set; }

        public uint DstZ { get; set; }

        public uint pSrcResource { get; set; }

        public uint SrcSubresource { get; set; }

        public uint pSrcBox { get; set; }

        public Slot.Resource Src { get; set; }

        public Slot.Resource Dst { get; set; }
    }
}
