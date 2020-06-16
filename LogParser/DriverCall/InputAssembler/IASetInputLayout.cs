namespace Migoto.Log.Parser.DriverCall
{
    public class IASetInputLayout : Base
    {
        public IASetInputLayout(DrawCall owner) : base(owner)
        {
        }

        public uint pInputLayout { get; set; }
    }
}
