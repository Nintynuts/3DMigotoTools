using Migoto.Log.Parser;
using Migoto.Log.Parser.ApiCalls;

using System;
using System.Collections.Generic;
using System.IO;

namespace Migoto.Log.Converter
{
    class AutoConverter
    {
        private readonly DrawCallColumns columnGroups;
        private readonly IEnumerable<(ShaderType type, ShaderColumns columns)> shaders;
        private readonly Action<string> logger;
        private readonly FileSystemWatcher watcher;

        public AutoConverter(string rootFolder, DrawCallColumns columnGroups, IEnumerable<(ShaderType type, ShaderColumns columns)> shaders, Action<string> logger)
        {
            this.columnGroups = columnGroups;
            this.shaders = shaders;
            this.logger = logger;
            watcher = new FileSystemWatcher(rootFolder, "FrameAnalysis-*");
            watcher.NotifyFilter = NotifyFilters.DirectoryName;
            watcher.Created += Created;
            watcher.EnableRaisingEvents = true;
        }

        public void Quit()
        {
            watcher.Created -= Created;
            watcher.Dispose();
        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            string inputFilePath = Path.Combine(e.FullPath, "log.txt");

            StreamReader frameAnalysisFile = null;
            while (frameAnalysisFile == null)
                try { frameAnalysisFile = new StreamReader(inputFilePath); } catch { }

            using var loggingFile = new StreamWriter(Path.Combine(e.FullPath, "3DMT-log.txt"));
            var frameAnalysis = new FrameAnalysis(frameAnalysisFile, msg => loggingFile.WriteLine(msg));
            frameAnalysis.Parse();
            loggingFile.Close();

            LogWriter.Write(frameAnalysis.Frames, LogWriter.GetOutputFileFrom(inputFilePath), columnGroups, shaders);
            logger($"Log converted: {e.Name}");
        }
    }
}
