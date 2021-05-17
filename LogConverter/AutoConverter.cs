using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    class AutoConverter
    {
        private readonly MigotoData data;
        private readonly IUserInterface ui;
        private readonly FileSystemWatcher frameAnalysisWatcher;
        private readonly FileSystemWatcher iniFileWatcher;

        public AutoConverter(string rootFolder, MigotoData data, IUserInterface ui)
        {
            this.data = data;
            this.ui = ui;
            frameAnalysisWatcher = new FileSystemWatcher(rootFolder, "FrameAnalysis-*")
            {
                NotifyFilter = NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };
            frameAnalysisWatcher.Created += FrameAnalysisCreated;

            iniFileWatcher = new FileSystemWatcher(rootFolder, "*.ini")
            {
                NotifyFilter = NotifyFilters.FileName,
                EnableRaisingEvents = true
            };
            iniFileWatcher.Created += ConfigCreated;
            iniFileWatcher.Changed += ConfigChanged;
        }

        public void Quit()
        {
            frameAnalysisWatcher.Created -= FrameAnalysisCreated;
            frameAnalysisWatcher.Dispose();
            iniFileWatcher.Created -= ConfigChanged;
            iniFileWatcher.Dispose();
        }

        private void FrameAnalysisCreated(object sender, FileSystemEventArgs e)
        {
            string inputFilePath = Path.Combine(e.FullPath, "log.txt");
            string outputFilePath = LogWriter.GetOutputFileFrom(inputFilePath);
            string logFilePath = Path.Combine(e.FullPath, "3DMT-log.txt");

            using var logging = IOHelpers.TryWriteFile(logFilePath, ui);
            using var output = IOHelpers.TryWriteFile(outputFilePath, ui);
            if (output != null && logging != null && data.LoadLog(inputFilePath, msg => logging.WriteLine(msg)))
            {
                LogWriter.Write(data, output);
                ui.Event($"Conversion success: {e.Name}");
                return;
            }
            ui.Event($"Conversion failure: {e.Name}");
        }

        private void ConfigCreated(object sender, FileSystemEventArgs e)
        {
            if (data.Config.Files.Any(f => f.WouldRecursivelyInclude(e.FullPath)))
                data.Config.Read(e.FullPath);
        }

        private void ConfigChanged(object sender, FileSystemEventArgs e)
        {
            data.Config.ReloadConfig(e.FullPath);
        }
    }
}
