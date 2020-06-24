using System.Collections.Generic;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public class RSSetViewports : MultiSlot<RSSetViewports, ResourceView>
    {
        public RSSetViewports(uint order) : base(order) { }

        public uint NumViewports { get => NumSlots; set => NumSlots = value; }
        public ulong pViewports { get => Pointer; set => Pointer = value; }

        public ICollection<ResourceView> Viewports => Slots;
    }
}
