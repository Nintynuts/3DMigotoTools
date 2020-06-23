using System;
using System.IO;

namespace Migoto.Log.Converter
{
    using Parser;

    class Program
    {
        static void Main(string[] args)
        {
            var file = new StreamReader(args[0]);

            var parser = new Parser(file, msg => Console.WriteLine(msg));

            var frames = parser.Parse();

            var fileName = args[0].Replace(".txt", ".csv");

            var output = TryOpenFile(fileName);

            LogWriter.Write(frames, output);

            Console.WriteLine("Press Escape to exit");

            var folder = Path.GetDirectoryName(args[0]);
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
