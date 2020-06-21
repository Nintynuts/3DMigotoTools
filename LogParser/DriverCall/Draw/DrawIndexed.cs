﻿namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class DrawIndexed : Base, IDraw, IDrawIndexed
    {
        public DrawIndexed(uint order, DrawCall owner) : base(order, owner) { }

        public uint StartIndexLocation { get; set; }
        public uint BaseVertexLocation { get; set; }

        public uint? StartVertex => BaseVertexLocation;
        public uint? VertexCount => null;
        public uint? StartIndex => StartIndexLocation;
        public uint? IndexCount { get; set; }
        public uint? StartInstance => null;
        public uint? InstanceCount => null;
    }
}
