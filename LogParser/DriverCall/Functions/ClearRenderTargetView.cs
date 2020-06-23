namespace Migoto.Log.Parser.DriverCall
{
    public class ClearRenderTargetView : ClearBase
    {
        public ClearRenderTargetView(uint order) : base(order) { }
        public ulong pRenderTargetView { get; set; }
        public ulong ColorRGBA { get; set; }
    }
}
