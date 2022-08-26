using System.IO;
using System.Linq;

namespace Migoto.Log.Converter
{
    class Program
    {
        static void Main(string[] args) => _ = new LogConverter(new ConsoleInterface(), args?.AsEnumerable() ?? Enumerable.Empty<string>());
    }
}
