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
        private readonly IEnumerable<FileInfo>? recursiveIncludes;
        private readonly IEnumerable<FileInfo>? directIncludes;

        public const string Extension = ".ini";

        public ConfigFile(FileInfo ini, DirectoryInfo rootFolder)
        {
            var parser = new ConfigParser(ini.FullName, settings);

            File = ini;
            Namespace = ini.FullName.Replace(rootFolder.FullName + @"\", "").Replace(Extension, "");
            parentFolder = ini.Directory!;

            TextureOverrides = ParseOverrides<TextureOverride>(parser);
            ShaderOverrides = ParseOverrides<ShaderOverride>(parser);

            if (parser.GetSection("Rendering") is { } renderingSection)
                OverrideDirectory = renderingSection.GetValue<string>("override_directory");

            // Look for further includes
            if (parser.GetSection("Include") is not { } includeSection)
                return;

            directIncludes = includeSection.GetValues<string>("include").Select(parentFolder.File);
            IncludeRecursive = includeSection.GetValue<string>("include_recursive");
            ExcludeRecursive = includeSection.GetValue<string>("exclude_recursive");
            recursiveIncludes = GetFolderIncludes(parentFolder);
        }

        public FileInfo File { get; }
        public string Namespace { get; }
        public IEnumerable<TextureOverride> TextureOverrides { get; }
        public IEnumerable<ShaderOverride> ShaderOverrides { get; }
        public string? OverrideDirectory { get; }
        public string? IncludeRecursive { get; }
        public string? ExcludeRecursive { get; }
        public IEnumerable<FileInfo> RecursiveIncludes => recursiveIncludes.OrEmpty();
        public IEnumerable<FileInfo> DirectIncludes => directIncludes.OrEmpty();
        public IEnumerable<FileInfo> Includes => DirectIncludes.Concat(RecursiveIncludes).Distinct();

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

        private IEnumerable<FileInfo> GetFolderIncludes(DirectoryInfo currentDir)
        {
            // include recursive might not be defined, so skip if missing
            return IncludeRecursive is null ? Enumerable.Empty<FileInfo>()
                : currentDir.CreateSubdirectory(IncludeRecursive)
                            .GetFiles($"*{Extension}", SearchOption.AllDirectories)
                            .Where(p => !IsExcluded(p.FullName));
        }

        public bool WouldRecursivelyInclude(FileInfo ini)
            => IncludeRecursive != null && ini.FullName.Replace(parentFolder.FullName + @"\", "") is { } relativePath
               && relativePath.StartsWith(IncludeRecursive) && !IsExcluded(relativePath);

        private bool IsExcluded(string relativePath) => !string.IsNullOrWhiteSpace(ExcludeRecursive) && relativePath.Contains(ExcludeRecursive);
    }
}