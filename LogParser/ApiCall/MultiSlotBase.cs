using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public interface IMultiSlot
    {
        List<int> GlobalSlotsMask { get; }

        IEnumerable<IResourceSlot> Slots { get; }
    }

    public abstract class MultiSlotBase<This, TSlot, TFallback> : ApiCall, IMergable<This>, IMultiSlot
        where This : MultiSlotBase<This, TSlot, TFallback>
        where TSlot : Slot
        where TFallback : class, IDeferred<TFallback, DrawCall>
    {
        private readonly List<string> mergeWarnings = new List<string>();

        private List<int> slotsMask;
        private List<TSlot> slotsSet;

        protected MultiSlotBase(uint order) : base(order)
        {
            SlotsPopulated = new SlotCollection<MultiSlotBase<This, TSlot, TFallback>, TSlot>(this);
        }

        public uint StartSlot { get; set; }
        protected uint NumSlots { get; set; }
        protected ulong Pointer { get; set; }
        public List<ulong> PointersMerged { get; protected set; }

        protected ICollection<TSlot> SlotsPopulated { get; }

        private List<int> SlotsMask
            => slotsMask ??= Enumerable.Range((int)StartSlot, (int)NumSlots).ToList();

        private List<TSlot> SlotsSet
            => slotsSet ??= GlobalSlotsMask.OrderBy(i => i).Select(GetSlot).ToList();

        public abstract List<int> GlobalSlotsMask { get; }

        protected abstract Deferred<TFallback, DrawCall> PreviousDeferred { get; }

        IEnumerable<IResourceSlot> IMultiSlot.Slots => SlotsSet.Cast<IResourceSlot>();

        private TSlot GetSlot(int index)
            => SlotsMask.Contains(index) ? SlotsPopulated.FirstOrDefault(s => s.Index == index) : GetPrevious(index);

        private TSlot GetPrevious(int index)
        {
            var slot = PreviousDeferred?.OfType<This>().FirstOrDefault()?.SlotsSet.Find(s => s?.Index == index);
            slot?.SetLastUser(this);
            return slot;
        }

        public IEnumerable<string> MergeWarnings => mergeWarnings;

        public virtual void Merge(This other)
        {
            for (uint i = 0; i < other.NumSlots; i++)
            {
                var slotIdx = (int)(other.StartSlot + i);
                if (SlotsMask.Contains(slotIdx))
                    mergeWarnings.Add($"{Name}: Overwriting slot {slotIdx}");
                else
                    SlotsMask.Add(slotIdx);
            }
            SlotsPopulated.Where(s => s.Index >= other.StartSlot && s.Index < other.StartSlot + other.NumSlots)
                .ToList().ForEach(s => SlotsPopulated.Remove(s));

            PointersMerged ??= new List<ulong> { Pointer };
            PointersMerged.Add(other.Pointer);
        }
    }
}
