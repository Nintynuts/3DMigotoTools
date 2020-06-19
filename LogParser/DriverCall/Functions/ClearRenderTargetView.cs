namespace Migoto.Log.Parser.DriverCall
{
    public class ClearRenderTargetView : Base
    {
        public ClearRenderTargetView(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint pRenderTargetView { get; set; }
        public uint ColorRGBA { get; set; }
    }
}
