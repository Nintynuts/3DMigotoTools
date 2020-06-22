using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class Map : Base, IResource
    {
        public Map(uint order, DrawCall owner) : base(order, owner) { }

        public ulong pResource { get; set; }
        public uint Subresource { get; set; }
        public uint MapType { get; set; }
        public uint MapFlags { get; set; }
        public ulong pMappedResource { get; set; }

        public Asset.Base Asset { get; private set; }
        ulong IResource.Pointer => pResource;
        Base IResource.Owner => this;

        public void UpdateAsset(Asset.Base asset) => Asset = asset;
    }
}
