namespace Migoto.Log.Parser
{
    public interface IOverriden<T>
    {
        T LastUser { get; }

        void SetLastUser(T lastUser);
    }
}
