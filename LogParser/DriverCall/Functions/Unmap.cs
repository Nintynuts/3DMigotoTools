using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class Unmap : Base, IResource
    {
        public Unmap(uint order, DrawCall owner) : base(order, owner) { }

        public ulong pResource { get; set; }
        public uint Subresource { get; set; }

        public Asset.Base Asset { get; private set; }
        int IResource.Index => -1;
        ulong IResource.Pointer => pResource;
        Base IResource.Owner => this;

        public void UpdateAsset(Asset.Base asset) => Asset = asset;
    }
}
