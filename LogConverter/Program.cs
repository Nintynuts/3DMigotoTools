using System;
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
                else if (inputFile.Extension == FrameAnalysis.Extension && GetValidLog(inputFile, out inputFile))
                {
                    OutputLog(loadedData, inputFile);
                    LogFunctions(inputFile);
                    return;
                }
            }

            while (ui.GetInfo("mode of operation", out var func))
            {
                switch (func.ToLower())
                {
                    case "manual":
                        if (GetValidLog(null, out var logFile) && logFile != null)
                            LogFunctions(logFile);
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
            var auto = new AutoConverter(inputFile.Directory!, loadedData, ui);
            ui.WaitForCancel("Watching for new FrameAnalysis export");
            auto.Quit();
        }

        private static bool GetValidLog(FileInfo? initial, [NotNullWhen(true)] out FileInfo file)
        {
            return ui.GetFile($"frame analysis log file (log{FrameAnalysis.Extension})", FrameAnalysis.Extension, initial, out file)
                && loadedData.LoadLog(file, ui.Event);
        }

        private static void LogFunctions(FileInfo inputFile)
        {
            while (loadedData.FrameAnalysis != null && ui.GetInfo("function to perform", out var func))
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLog(loadedData, inputFile); break;
                    case "asset":
                        OutputAsset(loadedData.FrameAnalysis, inputFile.Directory!); break;
                    case "set-columns":
                        loadedData.GetColumnSelection(); break;
                    case "get-metadata":
                        if (GetD3DXPath(null, out var d3dxPath))
                            loadedData.GetMetadata(d3dxPath);
                        break;
                }
            }
        }

        private static void OutputLog(MigotoData data, FileInfo file)
        {
            var outputFile = file.ChangeExt(CSV.Extension);
            using var output = outputFile.TryOpenWrite(ui);
            if (output == null)
                return;
            LogWriter.Write(data, output);
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

                using var assetFile = folder.File(asset.Hex + CSV.Extension).TryOpenWrite(ui);
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
