namespace Migoto.Log.Parser.DriverCall
{
    public class IASetPrimitiveTopology : Base
    {
        public IASetPrimitiveTopology(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public int Topology { get; set; }
    }
}
