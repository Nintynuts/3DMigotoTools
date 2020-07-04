using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Salaros.Configuration;

namespace Migoto.Config
{
    public class Config
    {
        public List<TextureOverride> TextureOverrides { get; private set; } = new List<TextureOverride>();
        public List<ShaderOverride> ShaderOverrides { get; private set; } = new List<ShaderOverride>();

        public void Read(string filePath)
        {
            TextureOverrides.Clear();
            ShaderOverrides.Clear();

            var settings = new ConfigParserSettings
            {
                MultiLineValues = MultiLineValues.AllowValuelessKeys
            };
            var iniSource = new ConfigParser(filePath, settings);
            var includeSection = iniSource.Sections.FirstOrDefault(s => s.SectionName == "Include");

            var includeFiles = includeSection.Keys.Where(k => k.Name == "include").Select(i => (string)i.ValueRaw);
            var includeFolder = (string)includeSection.Keys.FirstOrDefault(k => k.Name == "include_recursive").ValueRaw;
            var excludeFolder = (string)includeSection.Keys.FirstOrDefault(k => k.Name == "exclude_recursive").ValueRaw;

            string rootFolder = Directory.GetParent(filePath).FullName;
            var folder = Path.Combine(rootFolder, includeFolder);
            var iniFiles = Directory.GetFiles(folder, "*.ini").Where(p => !p.Contains(excludeFolder)).ToList();
            iniFiles.Add(filePath);

            var overrides =
                from iniFile in iniFiles
                let file = new ConfigParser(iniFile, settings)
                let @namespace = iniFile.Replace(rootFolder, "")
                let textures = file.Sections.Where(s => s.SectionName.StartsWith(nameof(TextureOverride)))
                    .Select(t => new TextureOverride
                    {
                        Name = t.SectionName.Replace(nameof(TextureOverride), "").Replace("_", " ").Trim(),
                        Namespace = @namespace,
                        Hash = uint.Parse((string)t.Keys.FirstOrDefault(k => k.Name.Equals(nameof(TextureOverride.Hash), System.StringComparison.OrdinalIgnoreCase))?.ValueRaw ?? "0", NumberStyles.HexNumber),
                        Lines = t.Lines.Select(l => l.Content).ToList()
                    })
                let shaders = file.Sections.Where(s => s.SectionName.StartsWith(nameof(ShaderOverride)))
                    .Select(t => new ShaderOverride
                    {
                        Name = t.SectionName.Replace(nameof(ShaderOverride), "").Replace("_", " ").Trim(),
                        Namespace = @namespace,
                        Hash = ulong.Parse((string)t.Keys.FirstOrDefault(k => k.Name.Equals(nameof(ShaderOverride.Hash), System.StringComparison.OrdinalIgnoreCase)).ValueRaw, NumberStyles.HexNumber),
                        Lines = t.Lines.Select(l => l.Content).ToList()
                    })
                select (textures, shaders);

            overrides.ToList().ForEach(o =>
            {
                TextureOverrides.AddRange(o.textures);
                ShaderOverrides.AddRange(o.shaders);
            });
        }
    }
}
