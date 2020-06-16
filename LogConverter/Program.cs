using System.IO;

namespace Migoto.Log.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = new StreamReader(args[0]);

            var parser = new Parser(file);

            parser.Parse();
        }
    }
}
