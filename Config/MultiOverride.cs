namespace Migoto.Config;

public class MultiOverride<T> : Override<T>
    where T : struct
{
    public MultiOverride(IGrouping<T, Override<T>> overrides)
    {
        Hash = overrides.Key;
        Name = overrides.Select(o => o.Name).ExceptNull().Delimit('/');
        Namespace = overrides.Select(o => o.Namespace).ExceptNull().Delimit('/');
        Lines = overrides.Select(o => o.Lines).ExceptNull().SelectMany(l => l).ToList();
    }

    public override string? HashFromString { set => throw new NotSupportedException(); }
}