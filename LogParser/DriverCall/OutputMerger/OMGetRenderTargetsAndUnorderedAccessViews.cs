namespace Migoto.Log.Parser.DriverCall
{
    public class OMGetRenderTargetsAndUnorderedAccessViews : Base
    {
        public OMGetRenderTargetsAndUnorderedAccessViews(DrawCall owner) : base(owner)
        {
        }

        public uint NumRTVs { get; set; }
        public uint ppRenderTargetViews { get; set; }
        public uint ppDepthStencilView { get; set; }
        public uint UAVStartSlot { get; set; }
        public uint NumUAVs { get; set; }
        public uint ppUnorderedAccessViews { get; set; }
    }
}
