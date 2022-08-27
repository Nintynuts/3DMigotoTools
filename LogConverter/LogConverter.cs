using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Parser;

    internal class LogConverter
    {
        private readonly IUserInterface ui;
        private readonly MigotoData loadedData = new();

        public LogConverter(IUserInterface ui, IEnumerable<string> input)
        {
            this.ui = ui;

            if (input.Any())
            {
                var path = input.First();
                input = input.Skip(1);

                loadedData.ColumnData = InitialiseColumns(input);

                if (IOHelpers.ValidatePath(path, FrameAnalysis.Extension, @throw: false) is { } logFile)
                    ExportSingleLog(logFile);
                else if (IOHelpers.ValidatePath(path, MigotoData.D3DX, @throw: false) is { } d3dxFile)
                    LoadMetadata(d3dxFile);
                else
                    throw new InvalidDataException("Unknown file type");
            }
            else if (ui.GetFile(MigotoData.D3DX, MigotoData.D3DX) is { } d3dxFile)
            {
                LoadMetadata(d3dxFile);
            }
        }

        private OutputColumns? InitialiseColumns(IEnumerable<string> input)
        {
            if (input.Any() == true)
            {
                try
                {
                    return OutputColumns.Parse(input);
                }
                catch (InvalidDataException ide)
                {
                    ui.Event($"Invalid columns: {ide.Message}");
                }
            }
            return null;
        }

        private void ExportSingleLog(FileInfo logFile)
        {
            var possibleRoot = logFile.Directory?.Parent;
            if (possibleRoot?.GetFiles(MigotoData.D3DX).FirstOrDefault() is { } d3dxFile)
                GetMetadata(d3dxFile);

            if (LoadLog(logFile, ui.Event) is { } frameAnalysis)
            {
                OutputLogManual(frameAnalysis, logFile);
                LogFunctions(logFile, frameAnalysis);
            }
        }

        private void LoadMetadata(FileInfo d3dxFile)
        {
            ui.Event($"Loading: {d3dxFile.FullName}");
            GetMetadata(d3dxFile);
            ChooseMode(d3dxFile);
        }

        private void ChooseMode(FileInfo? d3dxFile = null)
        {
            while (ui.GetInfo("mode of operation") is { } func)
            {
                switch (func.ToLower())
                {
                    case "manual":
                        if (RequestLogFile() is { } logFile && LoadLog(logFile, ui.Event) is { } frameAnalysis)
                            LogFunctions(logFile, frameAnalysis);
                        break;
                    case "auto":
                        WatchFolder(d3dxFile);
                        break;
                }
            }
        }

        private void LogFunctions(FileInfo logFile, FrameAnalysis frameAnalysis)
        {
            while (ui.GetInfo("function to perform") is { } func)
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLogManual(frameAnalysis, logFile); break;
                    case "asset":
                        OutputAsset(frameAnalysis, logFile.Directory!); break;
                    case "set-columns":
                        loadedData.ColumnData = RequestColumns(); break;
                    case "get-metadata":
                        if (RequestD3dxFile() is { } d3dxFile)
                            GetMetadata(d3dxFile);
                        break;
                }
            }
        }

        private void WatchFolder(FileInfo? d3dxFile)
        {
            d3dxFile ??= RequestD3dxFile();
            if (d3dxFile == null || (loadedData.ColumnData ??= RequestColumns()) == null)
                return;

            loadedData.SplitFrames = RequestSplitFrames();

            var auto = new MigotoFileWatcher(d3dxFile.Directory!, loadedData);
            auto.FrameAnalysisCreated += (directory) =>
            {
                ui.Event($"Converting: {directory.Name}");
                var logFile = directory.File($"log{FrameAnalysis.Extension}");

                var infoFile = directory.File("conversion.log");
                using var infoStream = infoFile.TryOpenWrite(ui);
                if (infoStream != null && LoadLog(logFile, msg => infoStream.WriteLine(msg)) is { } frameAnalysis)
                {
                    OutputLog(frameAnalysis, logFile);
                    return;
                }
                ui.Event($"Conversion failure");
            };
            ui.WaitForCancel("Watching for new FrameAnalysis export");
            auto.Quit();
        }

        private FileInfo? RequestD3dxFile()
            => ui.GetFile(MigotoData.D3DX, MigotoData.D3DX);

        private FileInfo? RequestLogFile()
            => ui.GetFile($"frame analysis log file (log{FrameAnalysis.Extension})", FrameAnalysis.Extension);

        private OutputColumns? RequestColumns() => ui.GetValid("column selection (default: IA VS PS OM Logic)", input =>
        {
            if (input.Length == 0)
                return (OutputColumns.Default, null);

            try
            {
                return (OutputColumns.Parse(input.Split(' ', StringSplitOptions.RemoveEmptyEntries)), null);
            }
            catch (InvalidDataException ide)
            {
                return ((OutputColumns?)null, ide.Message);
            }
        });

        private SplitFrames RequestSplitFrames() => ui.GetValid("whether you want to split frames into separate files (Yes/No [default]/Both)", input =>
        {
            if (input.Length == 0)
                return (SplitFrames.No, null);

            var splitMode = Enum.GetValues<SplitFrames>().Cast<SplitFrames?>().FirstOrDefault(v => v.ToString()?.StartsWith(input, StringComparison.OrdinalIgnoreCase) == true);

            return (splitMode, splitMode.HasValue ? null : "Unrecognised option");
        }) ?? SplitFrames.No;

        public FrameAnalysis? LoadLog(FileInfo file, Action<string> logger)
        {
            ui.Event("Waiting for log to be readable...");
            using var frameAnalysisFile = file.TryOpenRead()!;
            ui.Event("Reading log...");
            var frameAnalysis = new FrameAnalysis(frameAnalysisFile, logger);

            if (!frameAnalysis.Parse())
            {
                logger("Provided file is not a 3DMigoto FrameAnalysis log file");
                return null;
            }
            loadedData.LinkOverrides(frameAnalysis);
            loadedData.LinkShaderFixes(frameAnalysis);

            return frameAnalysis;
        }

        private void GetMetadata(FileInfo d3dx)
        {
            loadedData.RootFolder = d3dx.Directory!;
            ui.Status("Reading metadata from ini config files...");
            var config = loadedData.Config;
            config.Read(d3dx, reset: true);
            ui.Event($"TextureOverrides: {config.TextureOverrides.Count()}\nShaderOverrides: {config.ShaderOverrides.Count()}");

            if (config.OverrideDirectory is null)
                return; // Unlikely to happen, as this is in the default d3dx.ini

            ui.Status("Reading metadata from shader fixes...");
            var shaderFixes = loadedData.ShaderFixes;
            shaderFixes.Scrape(loadedData.RootFolder.SubDirectory(config.OverrideDirectory));
            ui.Event($"Shaders: {shaderFixes.ShaderNames.Count}\nTexture Registers: {shaderFixes.Textures.Count}\nConstant Buffer Registers {shaderFixes.ConstantBuffers.Count}");
        }

        private void OutputLogManual(FrameAnalysis frameAnalysis, FileInfo logFile)
        {
            if (frameAnalysis.Frames.Count > 1)
                loadedData.SplitFrames = RequestSplitFrames();
            if ((loadedData.ColumnData ??= RequestColumns()) != null)
                OutputLog(frameAnalysis, logFile);
        }

        private void OutputLog(FrameAnalysis frameAnalysis, FileInfo file)
        {
            var outputFile = file.ChangeExt(CSV.Extension);
            new LogWriter(loadedData, frameAnalysis, suffix => outputFile.SuffixName(suffix).TryOpenWrite(ui)).Write();
            ui.Event($"Conversion success");
        }

        private void OutputAsset(FrameAnalysis frameAnalysis, DirectoryInfo folder)
        {
            while (ui.GetInfo("a resource hash to dump lifecycle for") is { } hex)
            {
                uint hash;
                try
                {
                    if (hex.Length != 8)
                        throw new InvalidDataException(nameof(hex));
                    hash = uint.Parse(hex, NumberStyles.HexNumber);
                }
                catch
                {
                    ui.Event($"Invalid hash: {hex} (must be 8 chars, alphanumetic hex)");
                    continue;
                }

                if (!frameAnalysis.Assets.TryGetValue(hash, out var asset))
                {
                    ui.Event($"Hash not found: {hex}");
                    continue;
                }

                try
                {
                    new AssetWriter(asset).Write(() => folder.File(asset.Hex + CSV.Extension).TryOpenWrite(ui));
                    ui.Event($"Export of {asset.Hex} complete");
                }
                catch
                {
                    ui.Event($"Export of {asset.Hex} failed");
                }
            }
            ui.Event("Export Asset aborted");
        }
    }
}
