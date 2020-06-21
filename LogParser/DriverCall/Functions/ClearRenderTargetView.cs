namespace Migoto.Log.Parser.DriverCall
{
    public class ClearRenderTargetView : ClearBase
    {
        public ClearRenderTargetView(uint order, DrawCall owner) : base(order, owner) { }
        public ulong pRenderTargetView { get; set; }
        public ulong ColorRGBA { get; set; }
    }
}
