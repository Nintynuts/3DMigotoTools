namespace Migoto.Log.Parser.Slots
{
    public interface ISlot<TOwner> : IOverriden<TOwner>
    {
        int Index { get; }
    }

    public abstract class Slot<TOwner> : IOwned<TOwner>, ISlot<TOwner>
    {
        public int Index { get; set; } = -1;

        public TOwner Owner { get; protected set; }
        public TOwner LastUser { get; private set; }

        public virtual void SetOwner(TOwner newOwner) => Owner = newOwner;

        public void SetLastUser(TOwner lastUser) => LastUser = lastUser;
    }
}
