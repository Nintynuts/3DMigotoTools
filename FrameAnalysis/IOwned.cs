namespace Migoto.Log.Parser
{
    public interface IOwned<T>
    {
        T Owner { get; }

        void SetOwner(T newOwner);
    }
}
