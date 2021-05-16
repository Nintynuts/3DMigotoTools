using System.Collections.Generic;
using System.IO;
using System.Linq;

using Salaros.Configuration;

namespace Migoto.Config
{
    public class Config
    {
        public List<TextureOverride> TextureOverrides { get; private set; } = new List<TextureOverride>();
        public List<ShaderOverride> ShaderOverrides { get; private set; } = new List<ShaderOverride>();

        private DirectoryInfo rootFolder;
        private readonly List<string> included = new List<string>();

        public void Read(string d3dxPath)
        {
            TextureOverrides.Clear();
            ShaderOverrides.Clear();

            rootFolder = Directory.GetParent(d3dxPath);
            var settings = new ConfigParserSettings { MultiLineValues = MultiLineValues.AllowValuelessKeys };

            RecurseIncludes(d3dxPath, settings);
        }

        private void RecurseIncludes(string iniPath, ConfigParserSettings settings)
        {
            // Skip already processed file
            if (included.Contains(iniPath))
                return;

            // Add this file
            var @namespace = iniPath.Replace(rootFolder.FullName + @"\", "").Replace(".ini", "");
            var parser = new ConfigParser(iniPath, settings);
            TextureOverrides.AddRange(ParseOverrides<TextureOverride>(parser, @namespace));
            ShaderOverrides.AddRange(ParseOverrides<ShaderOverride>(parser, @namespace));
            included.Add(@namespace);

            // Look for further includes
            if (!(parser.GetSection("Include") is { } includeSection))
                return;

            var currentDir = Directory.GetParent(iniPath);
            var includes = includeSection.GetValues<string>("include")
                .Select(i => Path.Combine(currentDir.FullName, i));

            // include recursive might not be defined, so skip if missing
            if (includeSection.GetValue<string>("include_recursive") is string includeFolder)
            {
                var excludeFolder = includeSection.GetValue<string>("exclude_recursive");
                var folder = currentDir.CreateSubdirectory(includeFolder);
                var folderIncludes = folder.GetFiles("*.ini", SearchOption.AllDirectories).Select(f => f.FullName)
                                           .Where(p => excludeFolder == null || !p.Contains(excludeFolder));
                includes = includes.Concat(folderIncludes);
            }

            // recurse for nested includes
            foreach (var include in includes.ToList().Distinct())
                RecurseIncludes(include, settings);
        }

        private static IEnumerable<T> ParseOverrides<T>(ConfigParser file, string @namespace)
            where T : Override, new()
        {
            const string Hash = nameof(Override<int>.Hash);
            string className = typeof(T).Name;
            return file.GetSections(className).Select(t => new T
            {
                Namespace = @namespace,
                Name = t.RemovePrefix(className),
                HashFromString = t.GetValue<string>(Hash),
                Lines = t.Keys.Where(k => !k.Name.Equals(Hash, System.StringComparison.OrdinalIgnoreCase)).Select(l => l.ToString()).ToList()
            });
        }
    }
}
