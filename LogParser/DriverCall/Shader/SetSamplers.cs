using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    using IMergableSlots = IMergableSlots<SetSamplers, Sampler>;

    public class SetSamplers : Base, IMergableSlots
    {
        public SetSamplers(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint StartSlot { get; set; }

        public uint NumSamplers { get; set; }

        public ulong ppSamplers { get; set; }

        public List<Sampler> Samplers { get; set; } = new List<Sampler>(16);

        List<Sampler> IMergableSlots.Slots => Samplers;
        uint IMergableSlots.NumSlots { get => NumSamplers; set => NumSamplers = value; }
        ulong IMergableSlots.Pointer => ppSamplers;
        List<ulong> IMergableSlots.PointersMerged { get; set; }

        public void Merge(SetSamplers value) => ((IMergableSlots)this).DoMerge(value);
    }
}
