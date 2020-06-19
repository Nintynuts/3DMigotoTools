using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetConstantBuffers : Base
    {
        public SetConstantBuffers(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint StartSlot { get; set; }

        public uint NumBuffers { get; set; }

        public ulong ppConstantBuffers { get; set; }

        public List<Resource> ConstantBuffers { get; set; } = new List<Resource>(14);
    }
}
