using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetViewports : Base
    {
        public RSSetViewports(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint NumViewports { get; set; }
        public ulong pViewports { get; set; }

        public List<ResourceView> Viewports { get; } = new List<ResourceView>();
    }
}
