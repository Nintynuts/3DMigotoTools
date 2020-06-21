namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class DrawIndexed : DrawIndexedCommon, IDraw
    {
        public DrawIndexed(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public virtual uint? IndexCount { get; set; }
        public virtual uint? StartInstance => null;
        public virtual uint? InstanceCount => null;
    }
}
