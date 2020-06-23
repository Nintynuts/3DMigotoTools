namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetScissorRects : Base
    {
        public RSSetScissorRects(uint order) : base(order) { }

        public uint NumRects { get; set; }
        public ulong pRects { get; set; }
    }
}
