namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetScissorRects : Base
    {
        public RSSetScissorRects(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint NumRects { get; set; }
        public ulong pRects { get; set; }
    }
}
