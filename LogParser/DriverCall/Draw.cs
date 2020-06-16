namespace Migoto.Log.Parser.DriverCall
{
    public class Draw : Base
    {
        public Draw(DrawCall owner) : base(owner)
        {
        }

        public uint VertexCount { get; set; }

        public uint StartVertexLocation { get; set; }
    }
}
