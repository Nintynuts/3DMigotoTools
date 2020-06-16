namespace Migoto.Log.Parser.DriverCall
{
    public class ClearDepthStencilView : Base
    {
        public ClearDepthStencilView(DrawCall owner) : base(owner)
        {
        }

        public uint pDepthStencilView { get; set; }
        public uint ClearFlags { get; set; }
        public float Depth { get; set; }
        public uint Stencil { get; set; }
    }
}
