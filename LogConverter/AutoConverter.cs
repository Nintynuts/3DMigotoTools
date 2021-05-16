using System.IO;

namespace Migoto.Log.Converter
{
    class AutoConverter
    {
        private readonly MigotoData data;
        private readonly IUserInterface ui;
        private readonly FileSystemWatcher watcher;

        public AutoConverter(string rootFolder, MigotoData data, IUserInterface ui)
        {
            this.data = data;
            this.ui = ui;
            watcher = new FileSystemWatcher(rootFolder, "FrameAnalysis-*")
            {
                NotifyFilter = NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };
            watcher.Created += Created;
        }

        public void Quit()
        {
            watcher.Created -= Created;
            watcher.Dispose();
        }

        private void Created(object sender, FileSystemEventArgs e)
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
    }
}
