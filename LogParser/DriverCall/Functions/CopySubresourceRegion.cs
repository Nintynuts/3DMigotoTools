namespace Migoto.Log.Parser.DriverCall
{
    public class CopySubresourceRegion : CopyResource
    {
        public CopySubresourceRegion(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint DstSubresource { get; set; }

        public uint DstX { get; set; }

        public uint DstY { get; set; }

        public uint DstZ { get; set; }

        public uint SrcSubresource { get; set; }

        public ulong pSrcBox { get; set; }
    }
}
