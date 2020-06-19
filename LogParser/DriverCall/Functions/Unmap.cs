﻿using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class Unmap : Base, IResource
    {
        public Unmap(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pResource { get; set; }
        public uint Subresource { get; set; }

        public Asset.Base Resource { get; set; }

        Asset.Base IResource.Asset => Resource;
        int IResource.Index => -1;
        ulong IResource.Pointer => pResource;
        Base IResource.Owner => this;

        public void UpdateAsset(Asset.Base asset) => Resource = asset;
    }
}
