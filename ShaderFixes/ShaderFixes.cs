using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Migoto.ShaderFixes
{
    public class ShaderUsage<T>
    {
        public List<ulong> Hashes { get; } = new List<ulong>();

        public T Thing { get; }

        public ShaderUsage(T thing, IEnumerable<ulong> hashes)
        {
            Thing = thing;
            Hashes.AddRange(hashes);
        }
    }

    public class ShaderFixes
    {
        private readonly Regex includePattern = new Regex("#include\\s+\"(?'path'.*?)\"");
        private readonly Regex constantBufferPattern = new Regex(@"cbuffer\s+(?'name'\w+)\s*:\s*register\(b(?'slot'\d+)\)");
        private readonly Regex textureMacroPattern = new Regex(@"TEXTURE(?:_CMP)?\(_(?'type'\w+),(?'name'\w+),\d+,(?'slot'\d+)\)");
        private readonly Regex texturePattern = new Regex(@"(?'name'\w+)\s*:\s*register\(t(?'slot'\d+)\)");

        private readonly List<ShaderUsage<string>> includes = new List<ShaderUsage<string>>();
        private readonly List<string> done = new List<string>();

        public List<ShaderFix> ShaderNames { get; } = new List<ShaderFix>();
        public List<ShaderUsage<Register>> ConstantBuffers { get; private set; } = new List<ShaderUsage<Register>>();
        public List<ShaderUsage<Register>> Textures { get; private set; } = new List<ShaderUsage<Register>>();

        public void Scrape(string rootPath)
        {
            string shaderfixes = Path.Combine(rootPath, "ShaderFixes");

            var files = Directory.GetFiles(shaderfixes, "*_replace.txt");

            foreach (var filePath in files)
            {
                var filename = Path.GetFileName(filePath);
                var hash = ulong.Parse(filename.Substring(0, filename.IndexOf('-')), NumberStyles.HexNumber);
                var reader = new StreamReader(filePath);
                ShaderNames.Add(new ShaderFix(filename, hash, reader.ReadLine().Replace("// ", "")));
                reader.Close();
                ParseFile(filePath, new[] { hash });
            }

            do
            {
                var uniqueIncludes = Consolidate(includes.Where(ui => !done.Contains(ui.Thing)));

                done.AddRange(uniqueIncludes.Select(i => i.Thing));
                includes.Clear();

                foreach (var include in uniqueIncludes)
                    ParseFile(Path.Combine(shaderfixes, include.Thing), include.Hashes);
            } while (includes.Any());

            ConstantBuffers = Consolidate(ConstantBuffers);
            Textures = Consolidate(Textures);
        }

        private void ParseFile(string filePath, IEnumerable<ulong> hashes)
        {
            var reader = new StreamReader(filePath);
            var shader = reader.ReadToEnd();

            includes.AddRange(includePattern.Matches(shader).Select(m => new ShaderUsage<string>(m.Groups["path"].Value, hashes)));
            reader.Close();

            ReadRegisters(ConstantBuffers, constantBufferPattern, shader, hashes);
            ReadRegisters(Textures, textureMacroPattern, shader, hashes);
            ReadRegisters(Textures, texturePattern, shader, hashes);
        }

        private void ReadRegisters(List<ShaderUsage<Register>> registers, Regex pattern, string hlsl, IEnumerable<ulong> hashes)
        {
            registers.AddRange(pattern.Matches(hlsl).Select(m => new ShaderUsage<Register>(new Register
            {
                Index = int.Parse(m.Groups["slot"].Value),
                Name = m.Groups["name"].Value + GetType(m)
            }, hashes)));

            static string GetType(Match m)
            {
                var type = m.Groups.FirstOrDefault(g => g.Name == "type");
                return type != null ? $" ({type.Value})" : "";
            }
        }

        private List<ShaderUsage<T>> Consolidate<T>(IEnumerable<ShaderUsage<T>> things)
            => things.GroupBy(t => t.Thing).Select(t => new ShaderUsage<T>(t.Key, t.SelectMany(x => x.Hashes).ToArray())).ToList();
    }
}
