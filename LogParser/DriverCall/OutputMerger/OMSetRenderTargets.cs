using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class OMSetRenderTargets : Slots<OMSetRenderTargets, ResourceView>, IResourceSlots, IOutputMerger
    {
        public OMSetRenderTargets(uint order) : base(order) { }

        public uint NumViews { get => NumSlots; set => NumSlots = value; }

        public ulong ppRenderTargetViews { get => Pointer; set => Pointer = value; }

        public ICollection<ResourceView> RenderTargets => Slots;

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

        IEnumerable<ISlotResource> IResourceSlots.AllSlots => AllSlots.Cast<ISlotResource>();
    }
}
