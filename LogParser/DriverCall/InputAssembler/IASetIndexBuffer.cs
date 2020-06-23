namespace Migoto.Log.Parser.ApiCalls
{
    using Assets;

    public class IASetIndexBuffer : SingleSlot, IInputAssembler
    {
        public IASetIndexBuffer(uint order) : base(order) { }

        public ulong pIndexBuffer { get => Pointer; set => Pointer = value; }
        public uint Format { get; set; }
        public uint Offset { get; set; }

        public Buffer Buffer => Asset as Buffer;
    }
}
