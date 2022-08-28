namespace Migoto.Log.Parser;

public interface IMergable
{
    IEnumerable<string> MergeWarnings { get; }
}

public interface IMergable<T> : IMergable
{
    void Merge(T value);
}