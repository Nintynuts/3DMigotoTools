namespace Migoto.Log.Converter;

static class Program
{
    static void Main(string[] args) => _ = new LogConverter(new ConsoleInterface(), args?.AsEnumerable() ?? Enumerable.Empty<string>());
}
