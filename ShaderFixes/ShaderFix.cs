namespace Migoto.ShaderFixes
{
    public class ShaderFix
    {
        public ShaderFix(string filename, ulong hash, string name)
        {
            FileName = filename;
            Hash = hash;
            Name = name;
        }

        public string FileName { get; set; }

        public ulong Hash { get; set; }

        public string Name { get; set; }
    }

    public struct Register
    {
        public int Index { get; set; }

        public string Name { get; set; }
    }
}
