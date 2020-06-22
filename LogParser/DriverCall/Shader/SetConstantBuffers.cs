using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetConstantBuffers : ShaderSlots<SetConstantBuffers, Resource>, IResourceSlots
    {
        public SetConstantBuffers(uint order, DrawCall owner) : base(order, owner) { }

        public uint NumBuffers { get => NumSlots; set => NumSlots = value; }

        public ulong ppConstantBuffers { get => Pointer; set => Pointer = value; }

        public ICollection<Resource> ConstantBuffers => Slots;

        IEnumerable<ISlotResource> IResourceSlots.AllSlots => AllSlots.Cast<ISlotResource>();
    }
}
