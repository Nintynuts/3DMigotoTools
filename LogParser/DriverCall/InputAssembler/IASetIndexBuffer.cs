using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class IASetIndexBuffer : Base, IResource
    {
        public IASetIndexBuffer(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pIndexBuffer { get; set; }
        public uint Format { get; set; }
        public uint Offset { get; set; }

        public Buffer Buffer { get; set; }

        Asset.Base IResource.Asset => Buffer;
        int IResource.Index => (int)Offset;
        ulong IResource.Pointer => pIndexBuffer;
        Base IResource.Owner => this;

        public void UpdateAsset(Asset.Base asset) => Buffer = asset as Buffer;
    }
}
