using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetConstantBuffers : Base
    {
        public SetConstantBuffers(DrawCall owner) : base(owner)
        {
        }

        public uint StartSlot { get; set; }

        public uint NumBuffers { get; set; }

        public uint ppConstantBuffers { get; set; }

        public List<Resource> ConstantBuffers { get; set; } = new List<Resource>(14);
    }
}
