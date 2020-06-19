namespace Migoto.Log.Parser.DriverCall
{
    public class Map : Base
    {
        public Map(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint pResource { get; set; }
        public uint Subresource { get; set; }
        public uint MapType { get; set; }
        public uint MapFlags { get; set; }
        public uint pMappedResource { get; set; }

        public Asset.Base Resource { get; set; }
    }
}
