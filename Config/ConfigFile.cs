using System.Collections.Generic;
using System.IO;
using System.Linq;
using Salaros.Configuration;

namespace Migoto.Config
{
    public class ConfigFile
    {
        private static readonly ConfigParserSettings settings = new() { MultiLineValues = MultiLineValues.AllowValuelessKeys };
        private readonly DirectoryInfo parentFolder;
        private readonly IEnumerable<string>? recursiveIncludes;
        private readonly IEnumerable<string>? directIncludes;

        public ConfigFile(string path, DirectoryInfo rootFolder)
        {
            var parser = new ConfigParser(path, settings);

            FilePath = path;
            Namespace = path.Replace(rootFolder.FullName + @"\", "").Replace(".ini", "");
            parentFolder = new DirectoryInfo(IOHelpers.GetDirectoryName(path));

            TextureOverrides = ParseOverrides<TextureOverride>(parser);
            ShaderOverrides = ParseOverrides<ShaderOverride>(parser);

            if (parser.GetSection("Rendering") is { } renderingSection)
                OverrideDirectory = renderingSection.GetValue<string>("override_directory");

            // Look for further includes
            if (parser.GetSection("Include") is not { } includeSection)
                return;

            directIncludes = includeSection.GetValues<string>("include").Select(i => Path.Combine(parentFolder.FullName, i));
            IncludeFolder = includeSection.GetValue<string>("include_recursive");
            ExcludeFolder = includeSection.GetValue<string>("exclude_recursive");
            recursiveIncludes = GetFolderIncludes(parentFolder);
        }

        public string FilePath { get; }
        public string Namespace { get; }
        public IEnumerable<TextureOverride> TextureOverrides { get; }
        public IEnumerable<ShaderOverride> ShaderOverrides { get; }
        public string? OverrideDirectory { get; }
        public string? IncludeFolder { get; }
        public string? ExcludeFolder { get; }
        public IEnumerable<string> RecursiveIncludes => recursiveIncludes.OrEmpty();
        public IEnumerable<string> DirectIncludes => directIncludes.OrEmpty();
        public IEnumerable<string> Includes => DirectIncludes.Concat(RecursiveIncludes).Distinct();

        private IEnumerable<T> ParseOverrides<T>(ConfigParser file) where T : Override, new()
        {
            const string Hash = nameof(Override<int>.Hash);
            string className = typeof(T).Name;
            return file.GetSections(className).Select(t => new T
            {
                Namespace = Namespace,
                Name = t.RemovePrefix(className),
                HashFromString = t.GetValue<string>(Hash),
                Lines = new(t.Keys.Where(k => !k.Name.Equals(Hash, System.StringComparison.OrdinalIgnoreCase)).Select(l => l.ToString() ?? string.Empty))
            });
        }

        private IEnumerable<string> GetFolderIncludes(DirectoryInfo currentDir)
        {
            // include recursive might not be defined, so skip if missing
            return IncludeFolder is null ? Enumerable.Empty<string>()
                : currentDir.CreateSubdirectory(IncludeFolder)
                            .GetFiles("*.ini", SearchOption.AllDirectories)
                            .Select(f => f.FullName)
                            .Where(p => ExcludeFolder == null || !p.Contains(ExcludeFolder));
        }

        public bool WouldRecursivelyInclude(string iniPath)
            => IncludeFolder != null && iniPath.Replace(parentFolder.FullName + @"\", "") is { } relativePath
               && relativePath.StartsWith(IncludeFolder) && (ExcludeFolder == null || !relativePath.Contains(ExcludeFolder));
    }
}