namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetViewports : Base
    {
        public RSSetViewports(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint NumViewports { get; set; }
        public uint pViewports { get; set; }
    }
}
