using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Migoto.Config
{
    public class Config
    {
        private readonly ObservableCollection<ConfigFile> files = new();

        public IReadOnlyCollection<ConfigFile> Files => files;
        public IEnumerable<TextureOverride> TextureOverrides => files.SelectMany(c => c.TextureOverrides);
        public IEnumerable<ShaderOverride> ShaderOverrides => files.SelectMany(c => c.ShaderOverrides);

        public string? OverrideDirectory => files.Select(f => f.OverrideDirectory).ExceptNull().FirstOrDefault();

        public event Action<CollectionChange<TextureOverride>, CollectionChange<ShaderOverride>>? OverridesChanged;

        private DirectoryInfo? rootFolder;

        public Config()
        {
            files.CollectionChanged += (_, e) =>
            {
                var newConfigs = e.NewItems.OrEmpty<ConfigFile>();
                var oldConfigs = e.OldItems.OrEmpty<ConfigFile>();

                OverridesChanged?.Invoke(
                    new(oldConfigs.SelectMany(cfg => cfg.TextureOverrides), newConfigs.SelectMany(cfg => cfg.TextureOverrides)),
                    new(oldConfigs.SelectMany(cfg => cfg.ShaderOverrides), newConfigs.SelectMany(cfg => cfg.ShaderOverrides))
                );
            };
        }

        public void Read(string d3dxPath, bool reset = false)
        {
            if (reset)
                files.Clear();

            rootFolder = new(IOHelpers.GetDirectoryName(d3dxPath));

            RecurseIncludes(d3dxPath);
        }

        private void RecurseIncludes(string iniPath)
        {
            // Skip already processed file
            if (GetExistingConfig(iniPath) is not null || GetNewConfig(iniPath) is not { } configFile)
                return;

            // Add this file
            files.Add(configFile);

            // recurse for nested includes
            foreach (var include in configFile.Includes.Distinct())
                RecurseIncludes(include);
        }

        public void ReloadConfig(string iniPath)
        {
            if (GetNewConfig(iniPath) is not { } reloadedConfig)
                return;

            var existingConfig = GetExistingConfig(iniPath);
            if (existingConfig != null)
            {
                UpdateConfigs(reloadedConfig, existingConfig, cfg => cfg.DirectIncludes);

                if (reloadedConfig.IncludeFolder != existingConfig.IncludeFolder
                    || reloadedConfig.ExcludeFolder != existingConfig.ExcludeFolder)
                {
                    UpdateConfigs(reloadedConfig, existingConfig, cfg => cfg.RecursiveIncludes);
                }

                files.Remove(existingConfig);
            }
            files.Add(reloadedConfig);
        }

        private void UpdateConfigs(ConfigFile reloadedConfig, ConfigFile existingConfig, Func<ConfigFile, IEnumerable<string>> includes)
        {
            var oldConfigs = includes(existingConfig).Except(includes(reloadedConfig)).Select(GetExistingConfig).ExceptNull();
            oldConfigs.ForEach(c => files.Remove(c));

            var newConfigs = includes(reloadedConfig).Except(includes(existingConfig)).Select(GetNewConfig).ExceptNull();
            newConfigs.ForEach(files.Add);
        }

        private ConfigFile? GetNewConfig(string iniPath) => rootFolder is null ? null : new ConfigFile(iniPath, rootFolder);

        private ConfigFile? GetExistingConfig(string iniPath) => files.FirstOrDefault(cfg => cfg.FilePath == iniPath);
    }
}
