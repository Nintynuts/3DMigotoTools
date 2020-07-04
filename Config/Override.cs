
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Migoto.Config
{
    public class Override
    {
        private readonly static Regex camelCase = new Regex(@"(?<=[a-z])(?=[A-Z0-9])");

        public string Namespace { get; set; }

        public string Name { get; set; }

        public List<string> Lines { get; set; }

        public string FriendlyName => camelCase.Replace(Name, " ").Replace('_', ' ').Trim();
    }
}
