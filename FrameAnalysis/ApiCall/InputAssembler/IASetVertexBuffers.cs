namespace Migoto.Log.Parser.ApiCalls;

using Slots;

public class IASetVertexBuffers : MultiSlot<IASetVertexBuffers, Resource>, IInputAssembler
{
    public IASetVertexBuffers(uint order) : base(order) { }

    public uint NumBuffers { get => NumSlots; set => NumSlots = value; }
    public ulong ppVertexBuffers { get => Pointer; set => Pointer = value; }
    public ulong pStrides { get; set; }
    public ulong pOffsets { get; set; }

    public ICollection<Resource> VertexBuffers => SlotsPopulated;
}
