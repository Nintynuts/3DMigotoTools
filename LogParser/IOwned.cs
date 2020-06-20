namespace Migoto.Log.Parser
{
    interface IOwned<T>
    {
        T Owner { get; }

        void SetOwner(T newOwner);
    }
}
