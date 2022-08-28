namespace Migoto.Log.Parser.ApiCalls;

public class OMSetBlendState : ApiCall, IOutputMerger
{
    public OMSetBlendState(uint order) : base(order) { }

    public ulong pBlendState { get; set; }
    public ulong BlendFactor { get; set; }
    public uint SampleMask { get; set; }
}
