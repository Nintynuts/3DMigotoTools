namespace Migoto.Log.Parser.ApiCalls
{
    public class UpdateSubresource : SingleSlot
    {
        public UpdateSubresource(uint order) : base(order) { }

        public ulong pDstResource { get => Pointer; set => Pointer = value; }
        public uint DstSubresource { get; set; }
        public ulong pDstBox { get; set; }
        public ulong pSrcData { get; set; }
        public uint SrcRowPitch { get; set; }
        public uint SrcDepthPitch { get; set; }
    }
}
