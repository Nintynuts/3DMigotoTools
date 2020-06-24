namespace Migoto.Log.Parser.Slots
{
    using ApiCalls;

    public abstract class Slot : IOwned<ApiCall>, IOverriden<ApiCall>
    {
        public int Index { get; set; } = -1;

        public ApiCall Owner { get; protected set; }
        public ApiCall LastUser { get; private set; }

        public virtual void SetOwner(ApiCall newOwner) => Owner = newOwner;

        public void SetLastUser(ApiCall lastUser) => LastUser = lastUser;
    }
}
