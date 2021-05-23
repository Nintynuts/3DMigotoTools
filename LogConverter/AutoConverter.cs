using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Config;
    using Parser;

    class AutoConverter
    {
        private readonly MigotoData data;
        private readonly IUserInterface ui;
        private readonly FileSystemWatcher frameAnalysisWatcher;
        private readonly FileSystemEventHandler frameAnalyisCreated;
        private readonly FileSystemWatcher iniFileWatcher;
        private readonly FileSystemEventHandler configCreated;
        private readonly FileSystemEventHandler configChanged;

        public AutoConverter(DirectoryInfo root, MigotoData data, IUserInterface ui)
        {
            this.data = data;
            this.ui = ui;
            frameAnalysisWatcher = new FileSystemWatcher(root.FullName, "FrameAnalysis-*")
            {
                NotifyFilter = NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };
            frameAnalysisWatcher.Created += frameAnalyisCreated = IOHelpers.Handler<DirectoryInfo>(FrameAnalysisCreated);

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
            frameAnalysisWatcher.Created -= frameAnalyisCreated;
            frameAnalysisWatcher.Dispose();
            iniFileWatcher.Changed -= configChanged;
            iniFileWatcher.Created -= configCreated;
            iniFileWatcher.Dispose();
        }

        private void FrameAnalysisCreated(DirectoryInfo directory)
        {
            var inputFile = directory.File($"log{FrameAnalysis.Extension}");
            var outputFile = inputFile.ChangeExt(CSV.Extension);
            var logFile = directory.File("conversion.log");

            using var logging = logFile.TryOpenWrite(ui);
            using var output = outputFile.TryOpenWrite(ui);
            if (output != null && logging != null && data.LoadLog(inputFile, msg => logging.WriteLine(msg)))
            {
                LogWriter.Write(data, output);
                ui.Event($"Conversion success: {directory.Name}");
                return;
            }
            ui.Event($"Conversion failure: {directory.Name}");
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
}
