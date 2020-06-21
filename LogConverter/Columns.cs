using System;
using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser;
using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Converter
{
    internal interface IColumns
    {
        IEnumerable<string> Columns { get; }
        IEnumerable<string> GetValues(DrawCall dc);
    }

    internal class HashColumnSet : IColumns
    {
        private readonly string name;

        private readonly Func<DrawCall, IResourceSlots> provider;
        private readonly IEnumerable<int> columns;

        public HashColumnSet(string name, Func<DrawCall, IResourceSlots> provider, IEnumerable<int> columns)
        {
            this.name = name;
            this.provider = provider;
            this.columns = columns.OrderBy(i => i);
        }

        public IEnumerable<string> Columns => columns.Select(i => $"{name}{i}");

        public IEnumerable<string> GetValues(DrawCall dc)
        {
            var items = provider(dc);
            if (items == null)
                return Columns.Select(_ => string.Empty);

            return items.AllSlots.Select(r => r == null ? string.Empty : r.Asset?.Hex ?? "No Hash");
        }
    }

    internal class HashColumn : IColumns
    {
        private readonly string name;

        private readonly Func<DrawCall, IHash> provider;

        public HashColumn(string name, Func<DrawCall, IHash> provider)
        {
            this.name = name;
            this.provider = provider;
        }

        public IEnumerable<string> Columns => new[] { name };

        public IEnumerable<string> GetValues(DrawCall dc) => new[] { provider(dc)?.Hex ?? string.Empty };
    }

    internal class Column : IColumns
    {
        private readonly string name;

        private readonly Func<DrawCall, object> provider;

        public Column(string name, Func<DrawCall, object> provider)
        {
            this.name = name;
            this.provider = provider;
        }

        public IEnumerable<string> Columns => new[] { name };

        public IEnumerable<string> GetValues(DrawCall dc) => new[] { provider(dc)?.ToString() ?? string.Empty };
    }
}
