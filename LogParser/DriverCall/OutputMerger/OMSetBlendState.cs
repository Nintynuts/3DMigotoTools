namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetBlendState : Base
    {
        public OMSetBlendState(DrawCall owner) : base(owner)
        {
        }

        public uint pBlendState { get; set; }
        public int BlendFactor { get; set; }
        public uint SampleMask { get; set; }
    }
}
