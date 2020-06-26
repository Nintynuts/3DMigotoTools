using System;
using System.IO;

namespace Migoto.Log.Converter
{
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
                inputFilePath = Console.ReadLine().Replace("\"","");
            }

            var inputLog = new StreamReader(inputFilePath);

            var parser = new Parser(inputLog, msg => Console.WriteLine(msg));

            if (!parser.Parse())
            {
                Console.WriteLine("Provided file is not a 3DMigoto FrameAnalysis log file");
                return;
            }

            var outputFilePath = inputFilePath.Replace(".txt", ".csv");

            var outputCsv = TryOpenFile(outputFilePath);

            LogWriter.Write(parser.Frames, outputCsv);

            Console.WriteLine("Press Escape to exit");

            var folder = Path.GetDirectoryName(inputFilePath);
            do
            {
                Console.Write("Enter a resource hash to dump lifecycle for: ");
                var hash = Console.ReadLine().ToLower();

                if (parser.Assets.TryGetValue(hash, out var asset))
                {
                    var assetFile = TryOpenFile(Path.Combine(folder, $"{hash}.csv"));
                    try
                    {
                        AssetWriter.Write(asset, assetFile);
                        Console.WriteLine("Export complete");
                    }
                    catch (Exception e)
                    {
                        assetFile.Close();
                        Console.WriteLine("Export failed:");
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine("Hash not found");
                }

            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
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
    }
}
