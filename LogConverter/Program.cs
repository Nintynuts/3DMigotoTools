using System.IO;

using Migoto.Log.Converter;

namespace Migoto.Log.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = new StreamReader(args[0]);

            var parser = new Parser(file);

            var frames = parser.Parse();

            CsvWriter.Write(frames, args[0].Replace(".txt", ".csv"));
        }
    }
}
