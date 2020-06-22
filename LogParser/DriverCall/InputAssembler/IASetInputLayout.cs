namespace Migoto.Log.Parser.DriverCall
{
    public class IASetInputLayout : Base, IInputAssembler
    {
        public IASetInputLayout(uint order, DrawCall owner) : base(order, owner) { }

        public ulong pInputLayout { get; set; }
    }
}
