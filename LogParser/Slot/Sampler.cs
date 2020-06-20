namespace Migoto.Log.Parser.Slot
{
    public class Sampler : IOwned<DriverCall.Base>
    {
        public ulong Handle { get; set; }

        public DriverCall.Base Owner { get; private set; }

        public void SetOwner(DriverCall.Base newOwner) => Owner = newOwner;

        public Sampler(DriverCall.Base owner) => Owner = owner;
    }
}
