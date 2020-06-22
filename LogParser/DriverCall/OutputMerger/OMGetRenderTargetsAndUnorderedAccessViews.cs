using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    using OMGetRTsAndUAVs = OMGetRenderTargetsAndUnorderedAccessViews;

    public class OMGetRenderTargetsAndUnorderedAccessViews : Slots<OMGetRTsAndUAVs, ResourceView>, IOutputMerger
    {
        public OMGetRenderTargetsAndUnorderedAccessViews(uint order, DrawCall owner) : base(order, owner) { }
        public uint NumRTVs { get; set; }
        public ulong ppRenderTargetViews { get; set; }
        public ulong ppDepthStencilView { get; set; }
        public uint UAVStartSlot { get => StartSlot; set => StartSlot = value; }
        public uint NumUAVs { get => NumSlots; set => NumSlots = value; }
        public ulong ppUnorderedAccessViews { get => Pointer; set => Pointer = value; }
        public ICollection<ResourceView> Outputs => Slots;
    }
}
