﻿namespace Migoto.Log.Parser.DriverCall
{
    public class RSSetState : Base
    {
        public RSSetState(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pRasterizerState { get; set; }
    }
}
