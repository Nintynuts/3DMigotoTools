namespace Migoto.Log.Parser
{
    internal interface IMergable<T>
    {
        void Merge(T value);
    }
}