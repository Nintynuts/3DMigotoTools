using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetRenderTargets : Base, IMergable<OMSetRenderTargets>
    {
        public OMSetRenderTargets(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint NumViews { get; set; }
        public ulong ppRenderTargetViews { get; set; }
        public ulong pDepthStencilView { get; set; }

        public List<ResourceView> RenderTargets { get; private set; } = new List<ResourceView>();

        public ResourceView D { get; set; }
        public ResourceView DepthStencil => D;

        public void Merge(OMSetRenderTargets value)
        {
            if (RenderTargets.Count < value.RenderTargets.Count)
            {
                RenderTargets.ForEach(rt => rt.SetOwner(null));
                ppRenderTargetViews = value.ppRenderTargetViews;
                RenderTargets = value.RenderTargets;
            }
            else
            {
                for (int i = 0; i < value.RenderTargets.Count; i++)
                {
                    RenderTargets[i].SetOwner(null);
                    RenderTargets[i].Asset.Uses.Remove(RenderTargets[i]);
                    RenderTargets[i] = value.RenderTargets[i];
                }
            }
            value.RenderTargets.ForEach(rt => rt.SetOwner(this));

            if (value.DepthStencil != null)
            {
                pDepthStencilView = value.pDepthStencilView;
                DepthStencil?.SetOwner(null);
                D = value.D;
                DepthStencil.SetOwner(this);
            }
        }
    }
}
