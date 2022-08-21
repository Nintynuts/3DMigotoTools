using System.Linq;

namespace Migoto.Log.Converter
{
    class Program
    {
        static void Main(string[] args) => _ = new LogConverter(args?.AsEnumerable() ?? Enumerable.Empty<string>());
    }
}
