using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class IASetVertexBuffers : Slots<IASetVertexBuffers, Resource>, IResourceSlots
    {
        public IASetVertexBuffers(uint order, DrawCall owner) : base(order, owner) { }

        public uint NumBuffers { get => NumSlots; set => NumSlots = value; }

        public ulong ppVertexBuffers { get => Pointer; set => Pointer = value; }

        public ulong pStrides { get; set; }

        public ulong pOffsets { get; set; }

        public ICollection<Resource> VertexBuffers => Slots;

        IEnumerable<IResource> IResourceSlots.AllSlots => AllSlots.Cast<IResource>();
    }
}
