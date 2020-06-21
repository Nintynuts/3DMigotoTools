using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetViewports : Slots<RSSetViewports, ResourceView>
    {
        public RSSetViewports(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint NumViewports { get => NumSlots; set => NumSlots = value; }

        public ulong pViewports { get => Pointer; set => Pointer = value; }

        public ICollection<ResourceView> Viewports => Slots;
    }
}
