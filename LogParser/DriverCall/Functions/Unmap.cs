namespace Migoto.Log.Parser.DriverCall
{
    public class Unmap : Base
    {
        public Unmap(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint pResource { get; set; }
        public uint Subresource { get; set; }

        public Asset.Base Resource { get; set; }
    }
}
