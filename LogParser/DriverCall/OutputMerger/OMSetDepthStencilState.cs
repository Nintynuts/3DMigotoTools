namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetDepthStencilState : Base
    {
        public OMSetDepthStencilState(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pDepthStencilState { get; set; }
        public uint StencilRef { get; set; }
    }
}
