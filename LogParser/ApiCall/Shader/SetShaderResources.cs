using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public class SetShaderResources : ShaderMultiSlot<SetShaderResources, ResourceView>
    {
        public SetShaderResources(uint order) : base(order) { }

        public uint NumViews { get => NumSlots; set => NumSlots = value; }
        public ulong ppShaderResourceViews { get => Pointer; set => Pointer = value; }

        public ICollection<ResourceView> ResourceViews => SlotsPopulated;
    }
}
