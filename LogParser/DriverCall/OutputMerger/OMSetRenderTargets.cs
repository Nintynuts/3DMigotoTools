using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetRenderTargets : Base
    {
        public OMSetRenderTargets(DrawCall owner) : base(owner)
        {
        }

        public uint NumViews { get; set; }
        public uint ppRenderTargetViews { get; set; }
        public uint pDepthStencilView { get; set; }

        public List<ResourceView> RenderTargets { get; } = new List<ResourceView>();

        public ResourceView D { get; set; }
        public ResourceView DepthStencil => D;
    }
}
