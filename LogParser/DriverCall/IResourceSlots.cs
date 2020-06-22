using System.Collections.Generic;

using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public interface IResourceSlots : ISlotsUsage
    {
        IEnumerable<ISlotResource> AllSlots { get; }
    }
}
