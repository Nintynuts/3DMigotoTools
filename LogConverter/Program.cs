using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Parser;

    class Program
    {
        private static ConsoleInterface ui;
        private static MigotoData loadedData;

        static void Main(string[] args)
        {
            ui = new ConsoleInterface();
            loadedData = new MigotoData(ui);

            var input = args?.AsEnumerable() ?? Enumerable.Empty<string>();

            string inputFilePath = null;

            if (input.Any() && input.First().Contains("."))
            {
                inputFilePath = input.First();
                input = input.Skip(1);
            }

            loadedData.GetColumnSelection(input);

            if (inputFilePath != null)
            {
                if (inputFilePath.EndsWith(MigotoData.D3DX))
                {
                    if (GetD3DXPath(ref inputFilePath))
                        loadedData.GetMetadata(Path.GetDirectoryName(inputFilePath));
                }
                else if (inputFilePath.EndsWith(".txt") && GetValidLog(ref inputFilePath))
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
                        if (GetValidLog(ref logFile))
                            LogFunctions(logFile);
                        break;
                    case "auto":
                        if (GetD3DXPath(ref inputFilePath))
                            WatchFolder(inputFilePath);
                        break;
                }
            }
        }

        private static bool GetD3DXPath(ref string d3dxPath)
        {
            return ui.GetFile(MigotoData.D3DX, MigotoData.D3DX, ref d3dxPath);
        }

        private static void WatchFolder(string inputFilePath)
        {
            inputFilePath = Path.GetDirectoryName(inputFilePath);
            var auto = new AutoConverter(inputFilePath, loadedData, ui);
            ui.WaitForCancel("Watching for new FrameAnalysis export");
            auto.Quit();
        }

        private static bool GetValidLog(ref string path)
        {
            return ui.GetFile("frame analysis log file (log.txt)", ".txt", ref path)
                    && loadedData.LoadLog(path, ui.Event);
        }

        private static void LogFunctions(string inputFilePath)
        {
            while (ui.GetInfo("function to perform", out var func))
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLog(loadedData, inputFilePath); break;
                    case "asset":
                        OutputAsset(loadedData.FrameAnalysis, Path.GetDirectoryName(inputFilePath)); break;
                    case "set-columns":
                        loadedData.GetColumnSelection(); break;
                    case "get-metadata":
                        var d3dxPath = "";
                        while (!GetD3DXPath(ref d3dxPath))
                            loadedData.GetMetadata(Path.GetDirectoryName(d3dxPath));
                        break;
                }
            }
        }

        private static void OutputLog(MigotoData data, string inputFilePath)
        {
            var outputFile = LogWriter.GetOutputFileFrom(inputFilePath);
            using var output = IOHelpers.TryWriteFile(outputFile, ui);
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

                if (frameAnalysis.Assets.TryGetValue(hash, out var asset))
                {
                    using var assetFile = IOHelpers.TryWriteFile(Path.Combine(folder, $"{asset.Hex}.csv"), ui);
                    try
                    {
                        AssetWriter.Write(asset, assetFile);
                        ui.Event($"Export of {asset.Hex} complete");
                        return;
                    }
                    catch (Exception e)
                    {
                        ui.Event($"Export of {asset.Hex} failed:");
                        ui.Event(e.ToString());
                    }
                }
                else
                {
                    ui.Event($"Hash not found: {hex}");
                }
            }
            ui.Event("Export Asset aborted");
        }
    }
}
