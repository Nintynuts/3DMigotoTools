﻿namespace Migoto.Log.Parser.ApiCalls;

using Slots;

using OMGetRTsAndUAVs = OMGetRenderTargetsAndUnorderedAccessViews;

public class OMGetRenderTargetsAndUnorderedAccessViews : MultiSlot<OMGetRTsAndUAVs, ResourceView>, IOutputMerger
{
    public OMGetRenderTargetsAndUnorderedAccessViews(uint order) : base(order) { }
    public uint NumRTVs { get; set; }
    public ulong ppRenderTargetViews { get; set; }
    public ulong ppDepthStencilView { get; set; }
    public uint UAVStartSlot { get => StartSlot; set => StartSlot = value; }
    public uint NumUAVs { get => NumSlots; set => NumSlots = value; }
    public ulong ppUnorderedAccessViews { get => Pointer; set => Pointer = value; }
    public ICollection<ResourceView> Outputs => SlotsPopulated;
}
