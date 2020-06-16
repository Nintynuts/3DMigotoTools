using Migoto.Log.Parser.Asset;

namespace Migoto.Log.Parser.DriverCall
{
    public class IASetIndexBuffer : Base
    {
        public IASetIndexBuffer(DrawCall owner) : base(owner)
        {
        }

        public uint pIndexBuffer { get; set; }
        public uint Format { get; set; }
        public uint Offset { get; set; }

        public Buffer Buffer { get; set; }
    }
}
