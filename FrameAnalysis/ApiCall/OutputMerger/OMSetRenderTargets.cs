using System.Collections.Generic;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public class OMSetRenderTargets : MultiSlot<OMSetRenderTargets, ResourceView>, IOutputMerger
    {
        public OMSetRenderTargets(uint order) : base(order) { }

        public uint NumViews { get => NumSlots; set => NumSlots = value; }
        public ulong ppRenderTargetViews { get => Pointer; set => Pointer = value; }

        public ICollection<ResourceView> RenderTargets => SlotsPopulated;

        public override void Merge(OMSetRenderTargets value)
        {
            base.Merge(value);

            if (value.DepthStencil != null)
            {
                pDepthStencilView = value.pDepthStencilView;
                DepthStencil?.SetOwner(null);
                D = value.D;
                DepthStencil.SetOwner(this);
            }
        }

        public ulong pDepthStencilView { get; set; }
        public ResourceView D { get; set; }
        public ResourceView DepthStencil => D;
    }
}
