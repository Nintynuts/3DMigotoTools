namespace Migoto.Log.Parser.DriverCall.Draw
{
    public class DrawIndexedInstanced : DrawIndexed
    {
        public DrawIndexedInstanced(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public uint InstanceCount { get; set; }
        public uint IndexCountPerInstance { get; set; }
        public uint StartInstanceLocation { get; set; }
        public override uint? StartInstance => StartInstanceLocation;
        public override uint? EndInstance => StartInstance + InstanceCount;
    }
}
