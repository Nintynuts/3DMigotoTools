using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    using IMergableSlots = IMergableSlots<SetShaderResources, ResourceView>;

    public class SetShaderResources : Base, IMergableSlots
    {
        public SetShaderResources(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint StartSlot { get; set; }

        public uint NumViews { get; set; }

        public ulong ppShaderResourceViews { get; set; }

        public List<ResourceView> ResourceViews { get; set; } = new List<ResourceView>(16);

        List<ResourceView> IMergableSlots.Slots => ResourceViews;
        uint IMergableSlots.NumSlots { get => NumViews; set => NumViews = value; }
        ulong IMergableSlots.Pointer => ppShaderResourceViews;
        List<ulong> IMergableSlots.PointersMerged { get; set; }

        public void Merge(SetShaderResources value) => ((IMergableSlots)this).DoMerge(value);
    }
}
