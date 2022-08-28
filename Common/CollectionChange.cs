namespace System.Collections.Generic;

public struct CollectionChange<T>
{
    public CollectionChange(IEnumerable<T>? oldItems, IEnumerable<T>? newItems)
    {
        OldItems = oldItems.OrEmpty();
        NewItems = newItems.OrEmpty();
    }

    public IEnumerable<T> OldItems { get; }
    public IEnumerable<T> NewItems { get; }
}
