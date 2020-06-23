
using Migoto.Log.Parser.Asset;

namespace Migoto.Log.Parser.DriverCall
{
    public class IASetIndexBuffer : SingleSlotBase, IInputAssembler
    {
        public IASetIndexBuffer(uint order) : base(order) { }

        public ulong pIndexBuffer { get => Pointer; set => Pointer = value; }
        public uint Format { get; set; }
        public uint Offset { get; set; }

        public Buffer Buffer => Asset as Buffer;
    }
}
