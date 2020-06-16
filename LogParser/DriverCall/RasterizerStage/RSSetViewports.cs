namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetViewports : Base
    {
        public RSSetViewports(DrawCall owner) : base(owner)
        {
        }

        public uint NumViewports { get; set; }
        public uint pViewports { get; set; }
    }
}
