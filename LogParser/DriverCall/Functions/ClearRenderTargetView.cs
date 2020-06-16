namespace Migoto.Log.Parser.DriverCall
{
    public class ClearRenderTargetView : Base
    {
        public ClearRenderTargetView(DrawCall owner) : base(owner)
        {
        }

        public uint pRenderTargetView { get; set; }
        public uint ColorRGBA { get; set; }
    }
}
