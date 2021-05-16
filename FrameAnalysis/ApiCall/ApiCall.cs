namespace Migoto.Log.Parser.ApiCalls
{
    public interface IApiCall : IOwned<DrawCall>, IOverriden<DrawCall>, INamed
    {
        uint Order { get; }
    }

    public abstract class ApiCall : IApiCall
    {
        public uint Order { get; }
        public DrawCall? Owner { get; private set; }
        public DrawCall? LastUser { get; private set; }

        public void SetOwner(DrawCall? newOwner) => Owner = newOwner;
        public void SetLastUser(DrawCall lastUser) => LastUser = lastUser;

        public virtual string Name => GetType().Name;

        protected ApiCall(uint order)
        {
            Order = order;
        }
    }
}
