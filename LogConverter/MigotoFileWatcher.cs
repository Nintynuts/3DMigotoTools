namespace Migoto.Log.Converter;

using Config;

class MigotoFileWatcher
{
    private readonly MigotoData data;
    private readonly Dictionary<Action<DirectoryInfo>, FileSystemEventHandler> handlers = new();
    private readonly FileSystemWatcher frameAnalysisWatcher;
    private readonly FileSystemWatcher iniFileWatcher;
    private readonly FileSystemEventHandler configCreated;
    private readonly FileSystemEventHandler configChanged;

    public event Action<DirectoryInfo> FrameAnalysisCreated
    {
        add => frameAnalysisWatcher.Created += Register(value);
        remove => frameAnalysisWatcher.Created -= Unregister(value);
    }

    private FileSystemEventHandler Register(Action<DirectoryInfo> value)
    {
        var handler = IOHelpers.Handler(value);
        handlers[value] = handler;
        return handler;
    }

    private FileSystemEventHandler Unregister(Action<DirectoryInfo> value)
    {
        var handler = handlers[value];
        handlers.Remove(value);
        return handler;
    }

    public MigotoFileWatcher(DirectoryInfo root, MigotoData data)
    {
        this.data = data;
        frameAnalysisWatcher = new FileSystemWatcher(root.FullName, "FrameAnalysis-*")
        {
            NotifyFilter = NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };
        iniFileWatcher = new FileSystemWatcher(root.FullName, $"*{ConfigFile.Extension}")
        {
            NotifyFilter = NotifyFilters.FileName,
            EnableRaisingEvents = true
        };
        iniFileWatcher.Created += configCreated = IOHelpers.Handler<FileInfo>(ConfigCreated);
        iniFileWatcher.Changed += configChanged = IOHelpers.Handler<FileInfo>(ConfigChanged);
    }

    public void Quit()
    {
        handlers.ForEach(h => frameAnalysisWatcher.Created -= h.Value);
        frameAnalysisWatcher.Dispose();
        iniFileWatcher.Changed -= configChanged;
        iniFileWatcher.Created -= configCreated;
        iniFileWatcher.Dispose();
    }

    private void ConfigCreated(FileInfo file)
    {
        if (data.Config.Files.Any(f => f.WouldRecursivelyInclude(file)))
            data.Config.Read(file);
    }

    private void ConfigChanged(FileInfo file)
    {
        data.Config.ReloadConfig(file);
    }
}
