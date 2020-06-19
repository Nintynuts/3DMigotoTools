using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetRenderTargets : Base
    {
        public OMSetRenderTargets(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint NumViews { get; set; }
        public ulong ppRenderTargetViews { get; set; }
        public ulong pDepthStencilView { get; set; }

        public List<ResourceView> RenderTargets { get; } = new List<ResourceView>();

        public ResourceView D { get; set; }
        public ResourceView DepthStencil => D;
    }
}
