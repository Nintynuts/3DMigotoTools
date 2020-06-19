using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetShaderResources : Base
    {
        public SetShaderResources(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint StartSlot { get; set; }

        public uint NumViews { get; set; }

        public uint ppShaderResourceViews { get; set; }

        public List<ResourceView> ResourceViews { get; set; } = new List<ResourceView>(16);
    }
}
