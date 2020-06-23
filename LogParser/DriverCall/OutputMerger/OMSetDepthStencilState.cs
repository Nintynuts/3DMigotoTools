namespace Migoto.Log.Parser.ApiCalls
{
    public class OMSetDepthStencilState : ApiCall, IOutputMerger
    {
        public OMSetDepthStencilState(uint order) : base(order) { }

        public ulong pDepthStencilState { get; set; }
        public uint StencilRef { get; set; }
    }
}
