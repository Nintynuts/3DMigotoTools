namespace Migoto.Log.Parser.DriverCall
{
    using OMGetRTsAndUAVs = OMGetRenderTargetsAndUnorderedAccessViews;

    public class OMSetRenderTargetsAndUnorderedAccessViews : OMGetRTsAndUAVs, IOutputMerger
    {
        public OMSetRenderTargetsAndUnorderedAccessViews(uint order) : base(order) { }

        public ulong pDepthStencilView { get; set; }
        public ulong pUAVInitialCounts { get; set; }
    }
}
