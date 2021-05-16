namespace Migoto.Log.Parser
{
    public interface IOwned<T>
        where T : class
    {
        T? Owner { get; }

        void SetOwner(T? newOwner);
    }
}
