namespace Migoto.Log.Parser.DriverCall
{
    public class DrawIndexed : Base
    {
        public DrawIndexed(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint IndexCount { get; set; }
        public uint StartIndexLocation { get; set; }
        public uint BaseVertexLocation { get; set; }
    }
}
