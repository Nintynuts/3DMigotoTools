﻿namespace Migoto.Log.Parser.ApiCalls;

public class Map : Unmap
{
    public Map(uint order) : base(order) { }

    public uint MapType { get; set; }
    public uint MapFlags { get; set; }
    public ulong pMappedResource { get; set; }
}
