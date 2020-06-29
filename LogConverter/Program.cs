using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    using Migoto.Log.Parser;
    using Migoto.Log.Parser.ApiCalls;

    class Program
    {
        private static DrawCallColumns columns;
        private static List<(ShaderType type, ShaderColumns columns)> shaderColumns;
        private static FrameAnalysis frameAnalysis;

        static void Main(string[] args)
        {
            Console.WriteLine("Press Escape to cancel or exit");

            string inputFilePath = "";

            if (args?.Length > 0)
                inputFilePath = args[0];

            if (Directory.Exists(inputFilePath) || File.Exists(inputFilePath))
            {
                GetColumnSelection(args.Skip(1));
                if (GetWatchPath(out var validPath, inputFilePath))
                {
                    WatchFolder(validPath);
                }
                else if (LoadLog(out validPath, inputFilePath))
                {
                    OutputLog(frameAnalysis, validPath, columns, shaderColumns);
                    LogFunctions(inputFilePath);
                }
                return;
            }

            GetColumnSelection(args);

            while (ConsoleEx.ReadLineOrEsc("Enter mode of operation: ", out var func))
            {
                switch (func)
                {
                    case "manual":
                        while (LoadLog(out var validPath))
                            LogFunctions(validPath);
                        break;
                    case "auto":
                        while (GetWatchPath(out var validPath))
                            WatchFolder(validPath);
                        break;
                }
            }
        }

        private static void WatchFolder(string inputFilePath)
        {
            var auto = new AutoConverter(inputFilePath, columns, shaderColumns, Console.WriteLine);
            Console.WriteLine("Watching for new FrameAnalysis export...");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                continue;
            auto.Quit();
        }

        private static void LogFunctions(string inputFilePath)
        {
            while (ConsoleEx.ReadLineOrEsc("Enter function to perform: ", out var func))
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLog(frameAnalysis, inputFilePath, columns, shaderColumns); break;
                    case "asset":
                        OutputAsset(frameAnalysis, Path.GetDirectoryName(inputFilePath)); break;
                    case "set-columns":
                        GetColumnSelection(); break;
                }
            }
        }

        private static bool LoadLog(out string validFilePath, string path = "")
        {
            if (!IOHelpers.GetValidPath("input file (log.txt)", p => File.Exists(p), "File doesn't exist", out validFilePath, path))
                return false;

            frameAnalysis = new FrameAnalysis(new StreamReader(validFilePath), Console.WriteLine);

            if (!frameAnalysis.Parse())
            {
                Console.WriteLine("Provided file is not a 3DMigoto FrameAnalysis log file");
                return false;
            }

            return true;
        }

        private static bool GetWatchPath(out string watchFolderPath, string path = "")
        {
            return IOHelpers.GetValidPath("game executable (location of d3dx.ini)",
                p => Directory.Exists(p) && File.Exists(Path.Combine(p, "d3dx.ini")),
                "Directory doesn't exist, or d3dx.ini not present",
                out watchFolderPath, path);
        }

        private static void OutputLog(FrameAnalysis frameAnalysis, string inputFilePath, DrawCallColumns columns, List<(ShaderType type, ShaderColumns columns)> shaderColumns)
        {
            var outputCsv = LogWriter.GetOutputFileFrom(inputFilePath);

            LogWriter.Write(frameAnalysis.Frames, outputCsv, columns, shaderColumns);

            Console.WriteLine("Export Log complete");
        }

        private static bool GetColumnSelection(IEnumerable<string> cmdColumns = null)
        {
            IEnumerable<string> columnSelection;
            columns = DrawCallColumns.Index;
            shaderColumns = new List<(ShaderType type, ShaderColumns columns)>();
            if (cmdColumns?.Any() == true)
            {
                columnSelection = cmdColumns;
            }
            else
            {
                if (!ConsoleEx.ReadLineOrEsc("Please enter column selection (default: VB IB VS PS OM Logic): ", out var result))
                {
                    Console.WriteLine("Export Log aborted");
                    return false;
                }
                columnSelection = result.Split(' ');
            }

            if (columnSelection.Count() == 1 && columnSelection.First() == string.Empty)
                columnSelection = new[] { "All", "VS", "PS" };

            foreach (var column in columnSelection)
            {
                try
                {
                    var tokens = column.Split('-');
                    if (tokens.Length > 1)
                        shaderColumns.Add((ShaderTypes.FromLetter[tokens[0][0]], Enums.Parse<ShaderColumns>(tokens[1])));
                    else if (column.Last() == 's' || column.Last() == 'S')
                        shaderColumns.Add((ShaderTypes.FromLetter[column[0]], ShaderColumns.All));
                    else
                        columns |= Enums.Parse<DrawCallColumns>(column);
                }
                catch
                {
                    Console.WriteLine($"Failed to parse column: {column}");
                }
            }

            // Consolidate duplicate entries, just in case!
            shaderColumns = shaderColumns.GroupBy(s => s.type).Select(s => (s.Key, s.Select(c => c.columns).Aggregate((a, b) => a | b))).ToList();

            return true;
        }

        private static void OutputAsset(FrameAnalysis frameAnalysis, string folder)
        {
            while (ConsoleEx.ReadLineOrEsc("Enter a resource hash to dump lifecycle for: ", out var hash))
            {
                hash = hash.ToLower();

                if (frameAnalysis.Assets.TryGetValue(hash, out var asset))
                {
                    var assetFile = IOHelpers.TryOpenFile(Path.Combine(folder, $"{hash}.csv"));
                    try
                    {
                        AssetWriter.Write(asset, assetFile);
                        Console.WriteLine($"Export of {hash} complete");
                        return;
                    }
                    catch (Exception e)
                    {
                        assetFile.Close();
                        Console.WriteLine($"Export of {hash} failed:");
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
