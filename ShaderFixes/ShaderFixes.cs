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

        public const string Extension = "_replace.txt";

        public List<ShaderFix> ShaderNames { get; } = new List<ShaderFix>();
        public List<ShaderUsage<Register>> ConstantBuffers { get; private set; } = new List<ShaderUsage<Register>>();
        public List<ShaderUsage<Register>> Textures { get; private set; } = new List<ShaderUsage<Register>>();

        public void Scrape(DirectoryInfo shaderFixes)
        {
            var files = shaderFixes.GetFiles($"*{Extension}");

            foreach (var file in files)
            {
                var hash = ulong.Parse(file.Name.Substring(0, file.Name.IndexOf('-')), NumberStyles.HexNumber);
                using var reader = file.TryOpenRead();
                string? firstLine = reader?.ReadLine();
                if (firstLine == null)
                    continue; // can't open or empty file
                var name = firstLine.StartsWith("//") ? firstLine[2..] : hash.ToString();
                ShaderNames.Add(new ShaderFix(file.Name, hash, name.Trim()));
                ParseFile(file, new[] { hash });
            }

            do
            {
                var uniqueIncludes = Consolidate(includes.Where(ui => !done.Contains(ui.Thing)));

                done.AddRange(uniqueIncludes.Select(i => i.Thing));
                includes.Clear();

                foreach (var include in uniqueIncludes)
                    ParseFile(shaderFixes.File(include.Thing), include.Hashes);
            } while (includes.Any());

            ConstantBuffers = Consolidate(ConstantBuffers);
            Textures = Consolidate(Textures);
        }

        private void ParseFile(FileInfo file, IEnumerable<ulong> hashes)
        {
            using var reader = file.TryOpenRead();
            if (reader == null)
                return;
            var shader = reader.ReadToEnd();

            includes.AddRange(includePattern.Matches(shader).Select(m => new ShaderUsage<string>(m.Groups["path"].Value, hashes)));

            ReadRegisters(ConstantBuffers, constantBufferPattern);
            ReadRegisters(Textures, textureMacroPattern);
            ReadRegisters(Textures, texturePattern);

            void ReadRegisters(List<ShaderUsage<Register>> registers, Regex pattern)
            {
                registers.AddRange(pattern.Matches(shader).Select(m => new ShaderUsage<Register>(new Register
                {
                    Index = int.Parse(m.Groups["slot"].Value),
                    Name = m.Groups["name"].Value + GetType(m)
                }, hashes)));

                static string GetType(Match m)
                {
                    var type = m.Groups.Cast<Group>().FirstOrDefault(g => g.Name == "type");
                    return type != null ? $" ({type.Value})" : "";
                }
            }
        }

        private static List<ShaderUsage<T>> Consolidate<T>(IEnumerable<ShaderUsage<T>> things)
            => things.GroupBy(t => t.Thing).Select(t => new ShaderUsage<T>(t.Key, t.SelectMany(x => x.Hashes).ToArray())).ToList();
    }
}
