﻿namespace Migoto.Log.Parser.ApiCalls;

using Migoto.Log.Parser.Slots;

public class Unmap : ApiCall, ISingleSlot
{
    public Unmap(uint order) : base(order) { }

    public ulong pResource { get; set; }
    public uint Subresource { get; set; }

    public ResourceView? ResourceView { get; set; }

    IResource? ISingleSlot.Slot => ResourceView;
}
