using System;
using System.IO;

namespace Migoto.Log.Converter
{
    class AutoConverter
    {
        private readonly MigotoData data;
        private readonly Action<string> logger;
        private readonly FileSystemWatcher watcher;

        public AutoConverter(string rootFolder, MigotoData data, Action<string> logger)
        {
            this.data = data;
            this.logger = logger;
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

            using var loggingFile = new StreamWriter(Path.Combine(e.FullPath, "3DMT-log.txt"));
            data.LoadLog(inputFilePath, msg => loggingFile.WriteLine(msg));
            loggingFile.Close();

            LogWriter.Write(data.frameAnalysis.Frames, LogWriter.GetOutputFileFrom(inputFilePath), data.columns, data.shaderColumns);
            logger($"Log converted: {e.Name}");
        }
    }
}
