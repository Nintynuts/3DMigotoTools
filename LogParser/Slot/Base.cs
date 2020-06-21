namespace Migoto.Log.Parser.Slot
{
    public abstract class Base : IOwned<DriverCall.Base>
    {
        public int Index { get; set; } = -1;

        public DriverCall.Base Owner { get; protected set; }

        public virtual void SetOwner(DriverCall.Base newOwner) => Owner = newOwner;

        public Base(DriverCall.Base owner)
        {
            Owner = owner;
        }
    }
}
