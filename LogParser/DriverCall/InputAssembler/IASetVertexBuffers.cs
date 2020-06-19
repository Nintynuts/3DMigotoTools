using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class IASetVertexBuffers : Base
    {
        public IASetVertexBuffers(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint StartSlot { get; set; }
        public uint NumBuffers { get; set; }
        public uint ppVertexBuffers { get; set; }
        public uint pStrides { get; set; }
        public uint pOffsets { get; set; }

        public List<Resource> VertexBuffers { get; } = new List<Resource>();
    }
}
