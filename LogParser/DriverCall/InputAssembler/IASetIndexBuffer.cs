using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class IASetIndexBuffer : Base, IResource
    {
        public IASetIndexBuffer(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint pIndexBuffer { get; set; }
        public uint Format { get; set; }
        public uint Offset { get; set; }

        public Buffer Buffer { get; set; }


        public Asset.Base Asset => Buffer;
        public int Index => (int)Offset;
        public uint Pointer => pIndexBuffer;
        Base IResource.Owner => this;
    }
}
