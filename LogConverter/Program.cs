using System;
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

            string? inputFilePath = null;

            if (input.Any() && input.First().Contains("."))
            {
                inputFilePath = input.First();
                input = input.Skip(1);
            }

            if (inputFilePath == null)
                ui.GetFile(MigotoData.D3DX, MigotoData.D3DX, inputFilePath, out inputFilePath);

            loadedData.GetColumnSelection(input);

            if (inputFilePath != null)
            {
                if (inputFilePath.EndsWith(MigotoData.D3DX))
                {
                    if (GetD3DXPath(inputFilePath, out inputFilePath))
                        loadedData.GetMetadata(inputFilePath);
                }
                else if (inputFilePath.EndsWith(".txt") && GetValidLog(inputFilePath, out inputFilePath))
                {
                    OutputLog(loadedData, inputFilePath);
                    LogFunctions(inputFilePath);
                    return;
                }
            }

            while (ui.GetInfo("mode of operation", out var func))
            {
                switch (func.ToLower())
                {
                    case "manual":
                        var logFile = "";
                        if (GetValidLog(logFile, out logFile) && logFile != null)
                            LogFunctions(logFile);
                        break;
                    case "auto":
                        if (GetD3DXPath(inputFilePath, out inputFilePath))
                            WatchFolder(inputFilePath);
                        break;
                }
            }
        }

        private static bool GetD3DXPath(string? initial, out string d3dxPath)
        {
            return ui.GetFile(MigotoData.D3DX, MigotoData.D3DX, initial, out d3dxPath);
        }

        private static void WatchFolder(string inputFilePath)
        {
            inputFilePath = IOHelpers.GetDirectoryName(inputFilePath);
            var auto = new AutoConverter(inputFilePath, loadedData, ui);
            ui.WaitForCancel("Watching for new FrameAnalysis export");
            auto.Quit();
        }

        private static bool GetValidLog(string? initial, out string path)
        {
            return ui.GetFile("frame analysis log file (log.txt)", ".txt", initial, out path) && path != null
                    && loadedData.LoadLog(path, ui.Event);
        }

        private static void LogFunctions(string inputFilePath)
        {
            while (loadedData.FrameAnalysis != null && ui.GetInfo("function to perform", out var func))
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLog(loadedData, inputFilePath); break;
                    case "asset":
                        OutputAsset(loadedData.FrameAnalysis, IOHelpers.GetDirectoryName(inputFilePath)); break;
                    case "set-columns":
                        loadedData.GetColumnSelection(); break;
                    case "get-metadata":
                        var d3dxPath = "";
                        while (!GetD3DXPath(d3dxPath, out d3dxPath))
                            loadedData.GetMetadata(IOHelpers.GetDirectoryName(d3dxPath));
                        break;
                }
            }
        }

        private static void OutputLog(MigotoData data, string inputFilePath)
        {
            var outputFile = LogWriter.GetOutputFileFrom(inputFilePath);
            using var output = IOHelpers.TryWriteFile(outputFile, ui);
            if (output == null)
                return;
            LogWriter.Write(data, output);
            ui.Event("Export Log complete");
        }

        private static void OutputAsset(FrameAnalysis frameAnalysis, string folder)
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

                using var assetFile = IOHelpers.TryWriteFile(Path.Combine(folder, $"{asset.Hex}.csv"), ui);
                try
                {
                    if (assetFile != null)
                    {
                        AssetWriter.Write(asset, assetFile);
                        ui.Event($"Export of {asset.Hex} complete");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    ui.Event(e.ToString());
                }
                ui.Event($"Export of {asset.Hex} failed:");
            }
            ui.Event("Export Asset aborted");
        }
    }
}
