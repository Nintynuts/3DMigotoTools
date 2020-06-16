namespace Migoto.Log.Parser.DriverCall
{
    public class Unmap : Base
    {
        public Unmap(DrawCall owner) : base(owner)
        {
        }

        public uint pResource { get; set; }
        public uint Subresource { get; set; }

        public Asset.Base Resource { get; set; }
    }
}
