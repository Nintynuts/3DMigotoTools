namespace Migoto.Log.Parser.DriverCall
{
    public class IASetPrimitiveTopology : Base
    {
        public IASetPrimitiveTopology(DrawCall owner) : base(owner)
        {
        }

        public int Topology { get; set; }
    }
}
