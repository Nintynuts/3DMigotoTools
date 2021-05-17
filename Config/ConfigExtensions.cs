using System;
using System.Collections.Generic;
using System.Linq;

using Salaros.Configuration;

namespace Migoto.Config
{
    public static class ConfigExtensions
    {
        public static ConfigSection? GetSection(this ConfigParser parser, string name)
            => parser.Sections.FirstOrDefault(s => s.SectionName == name);

        public static IEnumerable<ConfigSection> GetSections(this ConfigParser parser, string name)
            => parser.Sections.Where(k => k.SectionName.StartsWith(name, StringComparison.OrdinalIgnoreCase));

        public static T? GetValue<T>(this ConfigSection section, string key)
            => (T)section.Keys.FirstOrDefault(k => k.IsMatch<T>(key))?.ValueRaw;

        public static IEnumerable<T> GetValues<T>(this ConfigSection section, string key)
            => section.Keys.Where(k => k.IsMatch<T>(key)).Select(k => (T)k.ValueRaw);

        private static bool IsMatch<T>(this IConfigKeyValue k, string key) => k.Name.Equals(key, StringComparison.OrdinalIgnoreCase) && k.ValueRaw is T;
        public static string RemovePrefix(this ConfigSection section, string name)
            => section.SectionName.Replace(name, "");
    }
}
