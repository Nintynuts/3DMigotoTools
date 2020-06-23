namespace Migoto.Log.Parser.ApiCalls
{
    public class RSSetState : ApiCall
    {
        public RSSetState(uint order) : base(order) { }

        public ulong pRasterizerState { get; set; }
    }
}
