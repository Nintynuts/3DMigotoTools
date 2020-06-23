namespace Migoto.Log.Parser.ApiCalls
{
    public class Unmap : SingleSlot
    {
        public Unmap(uint order) : base(order) { }

        public ulong pResource { get => Pointer; set => Pointer = value; }
        public uint Subresource { get; set; }
    }
}
