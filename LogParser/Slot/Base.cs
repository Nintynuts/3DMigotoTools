namespace Migoto.Log.Parser.Slot
{
    public abstract class Base : IOwned<DriverCall.Base>, IOverriden<DriverCall.Base>
    {
        public int Index { get; set; } = -1;

        public DriverCall.Base Owner { get; protected set; }
        public DriverCall.Base LastUser { get; private set; }

        public virtual void SetOwner(DriverCall.Base newOwner) => Owner = newOwner;

        public void SetLastUser(DriverCall.Base lastUser) => LastUser = lastUser;

        public Base(DriverCall.Base owner)
        {
            Owner = owner;
        }
    }
}
