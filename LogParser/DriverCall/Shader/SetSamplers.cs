﻿using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetSamplers : Base
    {
        public SetSamplers(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint StartSlot { get; set; }

        public uint NumSamplers { get; set; }

        public uint ppSamplers { get; set; }

        public List<Sampler> Samplers { get; set; } = new List<Sampler>(16);
    }
}
