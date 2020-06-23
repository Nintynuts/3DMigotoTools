using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class IASetVertexBuffers : Slots<IASetVertexBuffers, Resource>, IResourceSlots, IInputAssembler
    {
        public IASetVertexBuffers(uint order) : base(order) { }

        public uint NumBuffers { get => NumSlots; set => NumSlots = value; }

        public ulong ppVertexBuffers { get => Pointer; set => Pointer = value; }

        public ulong pStrides { get; set; }

        public ulong pOffsets { get; set; }

        public ICollection<Resource> VertexBuffers => Slots;

        IEnumerable<ISlotResource> IResourceSlots.AllSlots => AllSlots.Cast<ISlotResource>();
    }
}
