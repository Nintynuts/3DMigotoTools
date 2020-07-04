using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Parser;

    class Program
    {
        private static readonly MigotoData loadedData = new MigotoData();

        static void Main(string[] args)
        {
            var input = args?.AsEnumerable() ?? Enumerable.Empty<string>();
            Console.WriteLine("Press Escape to cancel or exit");

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
                    loadedData.GetMetadata(Path.GetDirectoryName(inputFilePath));
                }
                else if (inputFilePath.EndsWith(".txt") && GetValidLog(out var validPath, inputFilePath))
                {
                    OutputLog(loadedData, validPath);
                    LogFunctions(validPath);
                    return;
                }
            }

            while (ConsoleEx.ReadLineOrEsc("Enter mode of operation: ", out var func))
            {
                switch (func.ToLower())
                {
                    case "manual":
                        while (GetValidLog(out var validPath))
                            LogFunctions(validPath);
                        break;
                    case "auto":
                        while (GetWatchPath(out var validPath))
                            WatchFolder(validPath);
                        break;
                }
            }
        }

        private static bool GetWatchPath(out string watchFolderPath, string path = "")
        {
            return IOHelpers.GetValidPath($"game executable (location of {MigotoData.D3DX})",
                p => Directory.Exists(p) && File.Exists(Path.Combine(p, MigotoData.D3DX)),
                "Directory doesn't exist, or d3dx.ini not present",
                out watchFolderPath, path);
        }

        private static void WatchFolder(string inputFilePath)
        {
            var auto = new AutoConverter(inputFilePath, loadedData, Console.WriteLine);
            Console.WriteLine("Watching for new FrameAnalysis export...");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                continue;
            auto.Quit();
        }

        private static bool GetValidLog(out string validFilePath, string path = "")
        {
            return IOHelpers.GetValidPath("input file (log.txt)",
                    p => File.Exists(p), "File doesn't exist",
                    out validFilePath, path)
                    && loadedData.LoadLog(validFilePath, Console.WriteLine);
        }

        private static void LogFunctions(string inputFilePath)
        {
            while (ConsoleEx.ReadLineOrEsc("Enter function to perform: ", out var func))
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLog(loadedData, inputFilePath); break;
                    case "asset":
                        OutputAsset(loadedData.frameAnalysis, Path.GetDirectoryName(inputFilePath)); break;
                    case "set-columns":
                        loadedData.GetColumnSelection(); break;
                    case "get-metadata":
                        while (!ConsoleEx.ReadLineOrEsc($"Please enter {MigotoData.D3DX} location: ", out var d3dxPath))
                            loadedData.GetMetadata(Path.GetDirectoryName(d3dxPath));
                        break;
                }
            }
        }

        private static void OutputLog(MigotoData data, string inputFilePath)
        {
            var outputCsv = LogWriter.GetOutputFileFrom(inputFilePath);

            LogWriter.Write(data.frameAnalysis.Frames, outputCsv, data.columns, data.shaderColumns);

            Console.WriteLine("Export Log complete");
        }

        private static void OutputAsset(FrameAnalysis frameAnalysis, string folder)
        {
            while (ConsoleEx.ReadLineOrEsc("Enter a resource hash to dump lifecycle for: ", out var hex))
            {
                var hash = uint.Parse(hex, NumberStyles.HexNumber);

                if (frameAnalysis.Assets.TryGetValue(hash, out var asset))
                {
                    var assetFile = IOHelpers.TryOpenFile(Path.Combine(folder, $"{asset.Hex}.csv"));
                    try
                    {
                        AssetWriter.Write(asset, assetFile);
                        Console.WriteLine($"Export of {asset.Hex} complete");
                        return;
                    }
                    catch (Exception e)
                    {
                        assetFile.Close();
                        Console.WriteLine($"Export of {asset.Hex} failed:");
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine("Hash not found");
                }
            }
            Console.WriteLine("Export Asset aborted");
        }
    }
}
