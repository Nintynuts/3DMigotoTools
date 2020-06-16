namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetDepthStencilState : Base
    {
        public OMSetDepthStencilState(DrawCall owner) : base(owner)
        {
        }

        public uint pDepthStencilState { get; set; }
        public uint StencilRef { get; set; }
    }
}
