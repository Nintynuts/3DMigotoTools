using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.Asset
{
    public interface IHash
    {
        string Hex { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{GetType().Name}: {Hash.ToString(\"X\")}")]
    public abstract class Base : IHash
    {
        [TypeConverter(typeof(HashTypeConverter))]
        public uint Hash { get; set; }

        public string Hex => $"{Hash:X8}";

        public List<IResource> Uses { get; } = new List<IResource>();

        public List<(int index, List<ISlotResource> slots)> Slots => Uses.OfType<ISlotResource>().GroupBy(s => s.Index).Select(g => (index: g.Key, slots: g.ToList())).ToList();

        public List<DriverCall.Base> LifeCycle => Uses.Select(s => s.Owner).OrderBy(dc => dc.Owner.Index).ThenBy(dc => dc.Order).ToList();
    }
}
