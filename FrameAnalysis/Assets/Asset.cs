using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace Migoto.Log.Parser.Assets
{
    using ApiCalls;
    using Config;
    using ShaderFixes;
    using Slots;

    public interface IHash
    {
        string Hex { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{GetType().Name}: {Hash.ToString(\"X\")}")]
    public abstract class Asset : IHash, IConfigOverride<uint>
    {
        private readonly List<IResource> uses = new List<IResource>();

        [TypeConverter(typeof(HashTypeConverter))]
        public uint Hash { get; set; }

        public string Hex => $"{Hash:X8}";

        public IEnumerable<IResource> Uses => uses;
        public virtual void Register(IResource resource) => uses.Add(resource);
        public virtual void Unregister(IResource resource) => uses.Remove(resource);

        public List<(int index, List<IResourceSlot> slots)> Slots
            => Uses.OfType<IResourceSlot>().GroupBy(s => s.Index).OrderBy(g => g.Key).Select(g => (index: g.Key, slots: g.ToList())).ToList();

        public List<IApiCall> LifeCycle
            => Uses.Select(s => s.Owner).ExceptNull().OrderBy(dc => dc.Owner?.Owner?.Index).ThenBy(dc => dc.Owner?.Index).ThenBy(dc => dc.Order).ToList();

        public Override<uint>? Override { get; set; }

        public List<Register> VariableNames { get; } = new List<Register>();

        public string GetName(IApiCall? apiCall, int slot)
        {
            return Override?.FriendlyName
                ?? (apiCall is IShaderCall ? GetNameForSlot(slot) : null)
                ?? (VariableNames.Count > 0 ? CommonName : string.Empty);
        }

        private string CommonName => VariableNames.Select(v => v.Name).GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key;

        public string? GetNameForSlot(int slot)
        {
            return VariableNames.Any(v => v.Index == slot) ? VariableNames.First(v => v.Index == slot).Name : null;
        }
    }
}
