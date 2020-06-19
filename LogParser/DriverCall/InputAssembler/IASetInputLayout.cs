namespace Migoto.Log.Parser.DriverCall
{
    public class IASetInputLayout : Base
    {
        public IASetInputLayout(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pInputLayout { get; set; }
    }
}
