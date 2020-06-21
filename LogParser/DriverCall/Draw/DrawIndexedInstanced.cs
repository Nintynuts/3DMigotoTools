namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class DrawIndexedInstanced : DrawIndexedCommon, IDraw
    {
        public DrawIndexedInstanced(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint? InstanceCount { get; set; }
        public uint IndexCountPerInstance { get; set; }
        public uint StartInstanceLocation { get; set; }
        public uint? IndexCount => IndexCountPerInstance * InstanceCount;
        public uint? StartInstance => StartInstanceLocation;
    }
}
