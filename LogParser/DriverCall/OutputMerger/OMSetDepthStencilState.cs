namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetDepthStencilState : Base
    {
        public OMSetDepthStencilState(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint pDepthStencilState { get; set; }
        public uint StencilRef { get; set; }
    }
}
