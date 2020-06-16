namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetState : Base
    {
        public RSSetState(DrawCall owner) : base(owner)
        {
        }

        public uint pRasterizerState { get; set; }
    }
}
