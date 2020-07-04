namespace Migoto.Log.Parser.ApiCalls
{
    public class RSSetScissorRects : ApiCall
    {
        public RSSetScissorRects(uint order) : base(order) { }

        public uint NumRects { get; set; }
        public ulong pRects { get; set; }
    }
}
