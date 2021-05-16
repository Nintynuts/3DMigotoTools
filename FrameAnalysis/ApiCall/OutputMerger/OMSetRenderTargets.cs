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

            if (value.D != null)
            {
                pDepthStencilView = value.pDepthStencilView;
                D?.SetOwner(null);
                D = value.D;
                D.SetOwner(this);
            }
        }

        public ulong pDepthStencilView { get; set; }
        /// <summary> DepthStencil name in Log </summary>
        public ResourceView? D { get; set; }
        public ResourceView? DepthStencil => D;
    }
}
