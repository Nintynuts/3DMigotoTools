namespace Migoto.Log.Parser.DriverCall
{
    public class Unmap : SingleSlotBase
    {
        public Unmap(uint order) : base(order) { }

        public ulong pResource { get => Pointer; set => Pointer = value; }
        public uint Subresource { get; set; }
    }
}
