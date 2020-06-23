namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetBlendState : Base, IOutputMerger
    {
        public OMSetBlendState(uint order) : base(order) { }

        public ulong pBlendState { get; set; }
        public ulong BlendFactor { get; set; }
        public uint SampleMask { get; set; }
    }
}
