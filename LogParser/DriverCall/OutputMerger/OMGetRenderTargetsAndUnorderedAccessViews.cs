namespace Migoto.Log.Parser.DriverCall
{
    public class OMGetRenderTargetsAndUnorderedAccessViews : Base
    {
        public OMGetRenderTargetsAndUnorderedAccessViews(uint order, DrawCall owner) : base(order, owner)
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
