using System.Collections.Generic;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;
    public class SetConstantBuffers : ShaderMultiSlot<SetConstantBuffers, Resource>
    {
        public SetConstantBuffers(uint order) : base(order) { }

        public uint NumBuffers { get => NumSlots; set => NumSlots = value; }
        public ulong ppConstantBuffers { get => Pointer; set => Pointer = value; }

        public ICollection<Resource> ConstantBuffers => SlotsPopulated;
    }
}
