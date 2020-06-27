using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Migoto.Log.Converter
{
    using Parser.ApiCalls;
    using Parser;

    class Program
    {
        static void Main(string[] args)
        {
            string inputFilePath = "";
            if (args?.Length > 0)
                inputFilePath = args[0];

            while (inputFilePath == "" || !File.Exists(inputFilePath))
            {
                if (inputFilePath != "")
                    Console.WriteLine("File doesn't exist, please try again...");

                Console.Write("Please enter input file path (log.txt): ");
                inputFilePath = Console.ReadLine().Replace("\"", "");
            }

            var frameAnalysis = new FrameAnalysis(new StreamReader(inputFilePath), msg => Console.WriteLine(msg));

            if (!frameAnalysis.Parse())
            {
                Console.WriteLine("Provided file is not a 3DMigoto FrameAnalysis log file");
                return;
            }

            Console.WriteLine("Press Escape to exit");

            if (args.Length > 1)
                OutputLog(frameAnalysis, inputFilePath, args);

            while (ReadLineOrEsc("Enter function to perform: ", out var func))
            {
                switch (func.ToLower())
                {
                    case "log":
                        OutputLog(frameAnalysis, inputFilePath, args); break;
                    case "asset":
                        OutputAsset(frameAnalysis, Path.GetDirectoryName(inputFilePath)); break;
                }
            }
        }

        private static void OutputLog(FrameAnalysis frameAnalysis, string inputFilePath, string[] args)
        {
            IEnumerable<string> columnSelection;
            DrawCallColumns columns = DrawCallColumns.Index;
            var shaderColumns = new List<(ShaderType type, ShaderColumns columns)>();

            if (args.Length > 1)
            {
                columnSelection = args.Skip(1);
            }
            else
            {
                if (!ReadLineOrEsc("Please enter column selection (default: VB IB VS PS OM Logic): ", out var result))
                {
                    Console.WriteLine("Export Log aborted");
                    return;
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

            Regex frameAnalysisPattern = new Regex(@"(?<=FrameAnalysis([-\d]+)[\\/])(\w+)(?=\.txt)");
            if (frameAnalysisPattern.IsMatch(inputFilePath))
                inputFilePath = frameAnalysisPattern.Replace(inputFilePath, "$2$1");
            var outputFilePath = inputFilePath.Replace(".txt", ".csv");

            var outputCsv = TryOpenFile(outputFilePath);

            LogWriter.Write(frameAnalysis.Frames, outputCsv, columns, shaderColumns);

            Console.WriteLine("Export Log complete");
        }

        private static void OutputAsset(FrameAnalysis frameAnalysis, string folder)
        {
            while (ReadLineOrEsc("Enter a resource hash to dump lifecycle for: ", out var hash))
            {
                hash = hash.ToLower();

                if (frameAnalysis.Assets.TryGetValue(hash, out var asset))
                {
                    var assetFile = TryOpenFile(Path.Combine(folder, $"{hash}.csv"));
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

        private static StreamWriter TryOpenFile(string fileName)
        {
            StreamWriter output = null;
            do
            {
                try
                {
                    output = new StreamWriter(fileName);
                }
                catch (IOException)
                {
                    Console.WriteLine($"File: {fileName} in use, please close it and press any key to continue");
                }
            } while (output == null && Console.ReadKey() != null);
            return output;
        }

        private static bool ReadLineOrEsc(string message, out string result)
        {
            Console.Write(message);

            result = "";
            int curIndex = 0;
            var stroke = Console.ReadKey(true);
            while (stroke.Key != ConsoleKey.Escape && stroke.Key != ConsoleKey.Enter)
            {
                switch (stroke.Key)
                {
                    case ConsoleKey.Backspace:
                        if (curIndex > 0)
                        {
                            result = result.Remove(result.Length - 1);
                            Console.Write(stroke.KeyChar);
                            Console.Write(' ');
                            Console.Write(stroke.KeyChar);
                            curIndex--;
                        }
                        break;
                    default:
                        if (!char.IsControl(stroke.KeyChar))
                        {
                            result += stroke.KeyChar;
                            Console.Write(stroke.KeyChar);
                            curIndex++;
                        }
                        break;
                }
                stroke = Console.ReadKey(true);
            }
            ClearLine();
            return stroke.Key != ConsoleKey.Escape;
        }

        private static void ClearLine()
        {
            int currentLine = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLine);
        }
    }
}
