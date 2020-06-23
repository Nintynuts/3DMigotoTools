namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetDepthStencilState : Base, IOutputMerger
    {
        public OMSetDepthStencilState(uint order) : base(order) { }

        public ulong pDepthStencilState { get; set; }
        public uint StencilRef { get; set; }
    }
}
