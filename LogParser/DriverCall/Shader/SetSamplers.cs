using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetSamplers : ShaderSlots<SetSamplers, Sampler>
    {
        public SetSamplers(uint order) : base(order) { }

        public uint NumSamplers { get => NumSlots; set => NumSlots = value; }

        public ulong ppSamplers { get => Pointer; set => Pointer = value; }

        public ICollection<Sampler> Samplers => Slots;
    }
}
