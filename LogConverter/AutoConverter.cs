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

            using var loggingFile = new StreamWriter(logFilePath);
            if (data.LoadLog(inputFilePath, msg => loggingFile.WriteLine(msg)))
            {
                using var output = IOHelpers.TryWriteFile(outputFilePath, ui);
                LogWriter.Write(data, output);
                ui.Event($"Conversion success: {e.Name}");
            }
            else
            {
                ui.Event($"Conversion failure: {e.Name}");
            }
        }
    }
}
