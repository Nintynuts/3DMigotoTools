namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetBlendState : Base
    {
        public OMSetBlendState(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint pBlendState { get; set; }
        public int BlendFactor { get; set; }
        public uint SampleMask { get; set; }
    }
}
