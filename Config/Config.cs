namespace Migoto.Config;

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

    public void Read(FileInfo d3dx, bool reset = false)
    {
        if (reset)
            files.Clear();

        rootFolder = d3dx.Directory;

        RecurseIncludes(d3dx);
    }

    private void RecurseIncludes(FileInfo ini)
    {
        // Skip already processed file
        if (GetExistingConfig(ini) is not null || GetNewConfig(ini) is not { } configFile)
            return;

        // Add this file
        files.Add(configFile);

        // recurse for nested includes
        foreach (var include in configFile.Includes.Distinct())
            RecurseIncludes(include);
    }

    public void ReloadConfig(FileInfo ini)
    {
        if (GetNewConfig(ini) is not { } reloadedConfig)
            return;

        var existingConfig = GetExistingConfig(ini);
        if (existingConfig != null)
        {
            UpdateConfigs(reloadedConfig, existingConfig, cfg => cfg.DirectIncludes);

            if (reloadedConfig.IncludeRecursive != existingConfig.IncludeRecursive
                || reloadedConfig.ExcludeRecursive != existingConfig.ExcludeRecursive)
            {
                UpdateConfigs(reloadedConfig, existingConfig, cfg => cfg.RecursiveIncludes);
            }

            files.Remove(existingConfig);
        }
        files.Add(reloadedConfig);
    }

    private void UpdateConfigs(ConfigFile reloadedConfig, ConfigFile existingConfig, Func<ConfigFile, IEnumerable<FileInfo>> includes)
    {
        var oldConfigs = includes(existingConfig).Except(includes(reloadedConfig)).Select(GetExistingConfig).ExceptNull();
        oldConfigs.ForEach(c => files.Remove(c));

        var newConfigs = includes(reloadedConfig).Except(includes(existingConfig)).Select(GetNewConfig).ExceptNull();
        newConfigs.ForEach(files.Add);
    }

    private ConfigFile? GetNewConfig(FileInfo ini) => rootFolder is null ? null : new ConfigFile(ini, rootFolder);

    private ConfigFile? GetExistingConfig(FileInfo ini) => files.FirstOrDefault(cfg => cfg.File.FullName == ini.FullName);
}
