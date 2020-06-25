using System.Collections.Generic;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public class SetSamplers : ShaderMultiSlot<SetSamplers, Sampler>
    {
        public SetSamplers(uint order) : base(order) { }

        public uint NumSamplers { get => NumSlots; set => NumSlots = value; }
        public ulong ppSamplers { get => Pointer; set => Pointer = value; }

        public ICollection<Sampler> Samplers => SlotsPopulated;
    }
}
