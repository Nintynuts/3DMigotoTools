using System;
using System.IO;

using Migoto.Log.Converter;

namespace Migoto.Log.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = new StreamReader(args[0]);

            var parser = new Parser(file, msg => Console.WriteLine(msg));

            var frames = parser.Parse();

            var fileName = args[0].Replace(".txt", ".csv");

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

            CsvWriter.Write(frames, output);
        }
    }
}
