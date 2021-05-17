using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Migoto.Config
{
    public abstract class Override
    {
        private readonly static Regex camelCase = new Regex(@"(?<=[a-z])(?=[A-Z0-9])");

        public string? Namespace { get; set; }

        public string? Name { get; set; }

        public abstract string? HashFromString { set; }

        public List<string>? Lines { get; set; }

        public string FriendlyName => Name == null ? string.Empty : camelCase.Replace(Name, " ").Replace('_', ' ').Trim();
    }

    public abstract class Override<THash> : Override
        where THash : struct
    {
        public THash Hash { get; protected set; }
    }
}
