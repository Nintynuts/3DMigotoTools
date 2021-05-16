
namespace Migoto.Log.Parser.ApiCalls
{
    using Assets;

    public class UpdateSubresource : SingleSlot<Asset>
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
