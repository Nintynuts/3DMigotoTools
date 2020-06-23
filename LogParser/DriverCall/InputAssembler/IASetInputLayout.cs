namespace Migoto.Log.Parser.DriverCall
{
    public class IASetInputLayout : Base, IInputAssembler
    {
        public IASetInputLayout(uint order) : base(order) { }

        public ulong pInputLayout { get; set; }
    }
}
