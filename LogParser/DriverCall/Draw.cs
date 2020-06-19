namespace Migoto.Log.Parser.DriverCall
{
    public class Draw : Base
    {
        public Draw(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint VertexCount { get; set; }

        public uint StartVertexLocation { get; set; }
    }
}
