﻿using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Parser.Slot
{
    public class ResourceView : Resource
    {
        public ulong View { get; set; }

        public ResourceView(Base owner) : base(owner)
        {
        }
    }
}
