using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    using IMergableSlots = IMergableSlots<SetConstantBuffers, Resource>;

    public class SetConstantBuffers : Base, IMergableSlots
    {
        public SetConstantBuffers(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint StartSlot { get; set; }

        public uint NumBuffers { get; set; }

        public ulong ppConstantBuffers { get; set; }

        public List<Resource> ConstantBuffers { get; set; } = new List<Resource>(14);

        List<Resource> IMergableSlots.Slots => ConstantBuffers;
        uint IMergableSlots.NumSlots { get => NumBuffers; set => NumBuffers = value; }
        ulong IMergableSlots.Pointer => ppConstantBuffers;
        List<ulong> IMergableSlots.PointersMerged { get; set; }

        public void Merge(SetConstantBuffers value) => ((IMergableSlots)this).DoMerge(value);
    }
}
