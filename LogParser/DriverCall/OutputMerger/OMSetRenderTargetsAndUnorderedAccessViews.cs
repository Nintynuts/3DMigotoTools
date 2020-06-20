namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetRenderTargetsAndUnorderedAccessViews : OMGetRenderTargetsAndUnorderedAccessViews
    {
        public OMSetRenderTargetsAndUnorderedAccessViews(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pDepthStencilView { get; set; }
        public ulong pUAVInitialCounts { get; set; }
    }
}
