namespace Migoto.Log.Converter;

internal interface IColumns<T>
{
    IEnumerable<string> Columns { get; }
    IEnumerable<string> GetValues(T dc);
}

internal class ColumnSet<TContext, TItem> : IColumns<TContext>
{
    private readonly string name;

    private readonly Func<TContext, IReadOnlyList<TItem?>?> provider;
    private readonly Func<TContext, TItem?, string> selector;
    private readonly IEnumerable<int> columns;

    public ColumnSet(string name, Func<TContext, IReadOnlyList<TItem?>?> provider, Func<TContext, TItem?, string> selector, IEnumerable<int> columns)
    {
        this.name = name;
        this.provider = provider;
        this.selector = selector;
        this.columns = columns.OrderBy(i => i);
    }

    public IEnumerable<string> Columns => columns.Select(i => $"{name}{i}");

    public IEnumerable<string> GetValues(TContext ctx)
        => provider(ctx)?.Indices(columns).Select(i => selector(ctx, i)) ?? columns.Select(_ => string.Empty);
}

internal class Column<TContext, TItem> : IColumns<TContext>
{
    private readonly string name;

    private readonly Func<TContext, TItem> provider;
    private readonly Func<TContext, TItem, string> selector;

    public Column(string name, Func<TContext, TItem> provider, Func<TContext, TItem, string>? selector = null)
    {
        this.name = name;
        this.provider = provider;
        this.selector = selector ?? GetValue;
    }

    private static string GetValue(TContext ctx, TItem item) => item?.ToString() ?? string.Empty;

    public IEnumerable<string> Columns => new[] { name };

    public IEnumerable<string> GetValues(TContext ctx) => new[] { selector(ctx, provider(ctx)) };
}

internal static class CSV
{
    public const string Extension = ".csv";

    public static string ToCSV(this IEnumerable<string> items) => items.Delimit(',');
    public static string Headers<T>(this IEnumerable<IColumns<T>> items) => items.SelectMany(i => i.Columns).ToCSV();
    public static string Values<T>(this IEnumerable<IColumns<T>> items, T ctx) => items.SelectMany(i => i.GetValues(ctx)).ToCSV();
}
