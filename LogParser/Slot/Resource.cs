﻿using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Parser.Slot
{
    public class Resource
    {
        public uint Pointer { get; set; }

        public int Index { get; set; } = -1;

        public Asset.Base Asset { get; set; }

        public Base Owner { get; }

        public Resource(Base owner)
        {
            Owner = owner;
        }
    }
}
