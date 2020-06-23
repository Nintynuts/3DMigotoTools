namespace Migoto.Log.Parser.ApiCalls
{
    public class ClearRenderTargetView : Clear
    {
        public ClearRenderTargetView(uint order) : base(order) { }
        public ulong pRenderTargetView { get; set; }
        public ulong ColorRGBA { get; set; }
    }
}
