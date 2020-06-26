using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace Migoto.Log.Parser.Assets
{
    using ApiCalls;

    using Slots;

    public interface IHash
    {
        string Hex { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{GetType().Name}: {Hash.ToString(\"X\")}")]
    public abstract class Asset : IHash
    {
        private readonly List<IResource> uses = new List<IResource>();

        [TypeConverter(typeof(HashTypeConverter))]
        public uint Hash { get; set; }

        public string Hex => $"{Hash:X8}";

        public IEnumerable<IResource> Uses => uses;
        public void Register(IResource resource) => uses.Add(resource);
        public void Unregister(IResource resource) => uses.Remove(resource);

        public List<(int index, List<IResourceSlot> slots)> Slots
            => Uses.OfType<IResourceSlot>().GroupBy(s => s.Index).OrderBy(g => g.Key).Select(g => (index: g.Key, slots: g.ToList())).ToList();

        public List<IApiCall> LifeCycle
            => Uses.Select(s => s.Owner).OrderBy(dc => dc.Owner.Owner.Index).ThenBy(dc => dc.Owner.Index).ThenBy(dc => dc.Order).ToList();
    }
}
