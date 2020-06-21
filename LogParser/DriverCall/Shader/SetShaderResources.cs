using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetShaderResources : ShaderSlots<SetShaderResources, ResourceView>, IResourceSlots
    {
        public SetShaderResources(uint order, DrawCall owner) : base(order, owner) { }

        public uint NumViews { get => NumSlots; set => NumSlots = value; }

        public ulong ppShaderResourceViews { get => Pointer; set => Pointer = value; }

        public ICollection<ResourceView> ResourceViews => Slots;

        IEnumerable<IResource> IResourceSlots.AllSlots => AllSlots.Cast<IResource>();
    }
}
