using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class UpdateSubresource : Base, IResource
    {
        public UpdateSubresource(uint order, DrawCall owner) : base(order, owner) { }

        public ulong pDstResource { get; set; }
        public uint DstSubresource { get; set; }
        public ulong pDstBox { get; set; }
        public ulong pSrcData { get; set; }
        public uint SrcRowPitch { get; set; }
        public uint SrcDepthPitch { get; set; }

        public Asset.Base Asset { get; private set; }
        ulong IResource.Pointer => pDstResource;
        Base IResource.Owner => this;

        public void UpdateAsset(Asset.Base asset) => Asset = asset;
    }
}
