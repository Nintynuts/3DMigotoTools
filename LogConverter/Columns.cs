using System;
using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Converter
{
    internal interface IColumns<T>
    {
        IEnumerable<string> Columns { get; }
        IEnumerable<string> GetValues(T dc);
    }

    internal class HashColumnSet<T> : IColumns<T>
    {
        private readonly string name;

        private readonly Func<T, IResourceSlots> provider;
        private readonly IEnumerable<int> columns;

        public HashColumnSet(string name, Func<T, IResourceSlots> provider, IEnumerable<int> columns)
        {
            this.name = name;
            this.provider = provider;
            this.columns = columns.OrderBy(i => i);
        }

        public IEnumerable<string> Columns => columns.Select(i => $"{name}{i}");

        public IEnumerable<string> GetValues(T ctx)
        {
            var items = provider(ctx);
            if (items == null)
                return columns.Select(_ => string.Empty);

            return items.AllSlots.Select(r => r == null ? string.Empty : r.Asset?.Hex ?? "No Hash");
        }
    }

    internal class HashColumn<T> : IColumns<T>
    {
        private readonly string name;

        private readonly Func<T, IHash> provider;

        public HashColumn(string name, Func<T, IHash> provider)
        {
            this.name = name;
            this.provider = provider;
        }

        public IEnumerable<string> Columns => new[] { name };

        public IEnumerable<string> GetValues(T ctx) => new[] { provider(ctx)?.Hex ?? string.Empty };
    }

    internal class Column<T> : IColumns<T>
    {
        private readonly string name;

        private readonly Func<T, object> provider;

        public Column(string name, Func<T, object> provider)
        {
            this.name = name;
            this.provider = provider;
        }

        public IEnumerable<string> Columns => new[] { name };

        public IEnumerable<string> GetValues(T ctx) => new[] { provider(ctx)?.ToString() ?? string.Empty };
    }

    internal static class CsvExtensions
    {
        public static string Delimit(this IEnumerable<string> items, char delimiter)
            => items.Any() ? items.Aggregate((a, b) => $"{a}{delimiter}{b}") : string.Empty;
        public static string ToCSV(this IEnumerable<string> items) => items.Delimit(',');
        public static string Headers<T>(this IEnumerable<IColumns<T>> items) => items.SelectMany(i => i.Columns).ToCSV();
        public static string Values<T>(this IEnumerable<IColumns<T>> items, T ctx) => items.SelectMany(i => i.GetValues(ctx)).ToCSV();
    }
}
