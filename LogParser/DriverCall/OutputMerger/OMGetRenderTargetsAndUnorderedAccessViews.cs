using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class OMGetRenderTargetsAndUnorderedAccessViews : Base
    {
        public OMGetRenderTargetsAndUnorderedAccessViews(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint NumRTVs { get; set; }
        public ulong ppRenderTargetViews { get; set; }
        public ulong ppDepthStencilView { get; set; }
        public uint UAVStartSlot { get; set; }
        public uint NumUAVs { get; set; }
        public ulong ppUnorderedAccessViews { get; set; }

        public List<ResourceView> Outputs { get; } = new List<ResourceView>();
    }
}
