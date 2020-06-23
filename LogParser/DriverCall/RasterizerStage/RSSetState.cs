namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetState : Base
    {
        public RSSetState(uint order) : base(order) { }

        public ulong pRasterizerState { get; set; }
    }
}
