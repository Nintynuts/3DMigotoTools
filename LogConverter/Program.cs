using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Parser;

    class Program
    {
        private static readonly ConsoleInterface ui;
        private static readonly MigotoData loadedData;

        static Program()
        {
            ui = new ConsoleInterface();
            loadedData = new MigotoData(ui);
        }

        static void Main(string[] args)
        {
            var input = args?.AsEnumerable() ?? Enumerable.Empty<string>();

            FileInfo? inputFile = null;

            if (input.Any() && input.First() is { } path && path.Contains("."))
            {
                inputFile = new FileInfo(path);
                ui.Event($"Loading: {path}");
                input = input.Skip(1);
            }

            if (inputFile == null)
                ui.GetFile(MigotoData.D3DX, MigotoData.D3DX, inputFile, out inputFile);

            loadedData.GetColumnSelection(input);

            if (inputFile != null)
            {
                if (inputFile.Extension == MigotoData.D3DX)
                {
                    if (GetD3DXPath(inputFile, out inputFile))
                        loadedData.GetMetadata(inputFile);
                }
                else if (inputFile.Extension == FrameAnalysis.Extension && GetValidLog(inputFile, out inputFile) is { } frameAnalysis)
                {
                    OutputLog(loadedData, frameAnalysis, inputFile);
                    LogFunctions(inputFile, frameAnalysis);
                    return;
                }
            }

            while (ui.GetInfo("mode of operation", out var func))
            {
                switch (func.ToLower())
                {
                    case "manual":
                        if (GetValidLog(null, out var logFile) is { } frameAnalysis && logFile != null)
                            LogFunctions(logFile, frameAnalysis);
                        break;
                    case "auto":
                        if (GetD3DXPath(inputFile, out inputFile))
                            WatchFolder(inputFile);
                        break;
                }
            }
        }

        private static bool GetD3DXPath(FileInfo? initial, [NotNullWhen(true)] out FileInfo d3dxPath)
        {
            return ui.GetFile(MigotoData.D3DX, MigotoData.D3DX, initial, out d3dxPath);
        }

        private static void WatchFolder(FileInfo inputFile)
        {
            loadedData.GetSplitFile();
            var auto = new AutoConverter(inputFile.Directory!, loadedData, ui);
            ui.WaitForCancel("Watching for new FrameAnalysis export");
            auto.Quit();
        }

        private static FrameAnalysis? GetValidLog(FileInfo? initial, [NotNullWhen(true)] out FileInfo file)
        {
            return ui.GetFile($"frame analysis log file (log{FrameAnalysis.Extension})", FrameAnalysis.Extension, initial, out file)
                && loadedData.LoadLog(file, ui.Event) is { } frameAnalysis ? frameAnalysis : null;
        }

        private static void LogFunctions(FileInfo inputFile, FrameAnalysis frameAnalysis)
        {
            if (frameAnalysis.Frames.Count > 1)
                loadedData.GetSplitFile();

            while (ui.GetInfo("function to perform", out var func))
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLog(loadedData, frameAnalysis, inputFile); break;
                    case "asset":
                        OutputAsset(frameAnalysis, inputFile.Directory!); break;
                    case "set-columns":
                        loadedData.GetColumnSelection(); break;
                    case "get-metadata":
                        if (GetD3DXPath(null, out var d3dxPath))
                            loadedData.GetMetadata(d3dxPath);
                        break;
                }
            }
        }

        private static void OutputLog(MigotoData data, FrameAnalysis frameAnalysis, FileInfo file)
        {
            var outputFile = file.ChangeExt(CSV.Extension);
            new LogWriter(data, frameAnalysis, suffix => outputFile.SuffixName(suffix).TryOpenWrite(ui)).Write();
            ui.Event("Export Log complete");
        }

        private static void OutputAsset(FrameAnalysis frameAnalysis, DirectoryInfo folder)
        {
            while (ui.GetInfo("a resource hash to dump lifecycle for", out var hex))
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
