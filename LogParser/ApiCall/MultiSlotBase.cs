using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public interface IMultiSlot
    {
        List<int> SlotsUsed { get; }

        IEnumerable<IResourceSlot> AllSlots { get; }
    }

    public abstract class MultiSlotBase<This, TSlot, TFallback> : ApiCall, IMergable<This>, IMultiSlot
        where This : MultiSlotBase<This, TSlot, TFallback>
        where TSlot : Slot
        where TFallback : class, IDeferred<TFallback, DrawCall>
    {
        private readonly List<string> mergeWarnings = new List<string>();

        private List<int> slotsMask;
        private List<TSlot> allSlots;

        protected MultiSlotBase(uint order) : base(order)
        {
            Slots = new SlotCollection<MultiSlotBase<This, TSlot, TFallback>, TSlot>(this);
        }

        public abstract List<int> SlotsUsed { get; }

        protected abstract Deferred<TFallback, DrawCall> Deferred { get; }

        protected ICollection<TSlot> Slots { get; }
        public uint StartSlot { get; set; }
        protected uint NumSlots { get; set; }
        protected ulong Pointer { get; set; }
        public List<ulong> PointersMerged { get; protected set; }

        protected List<TSlot> AllSlots => allSlots ??= SlotsUsed.OrderBy(i => i).Select(GetSlot).ToList();

        IEnumerable<IResourceSlot> IMultiSlot.AllSlots => AllSlots.Cast<IResourceSlot>();

        private TSlot GetSlot(int index)
            => SlotsMask.Contains(index) ? Slots.FirstOrDefault(s => s.Index == index) : GetPrevious(index);

        private TSlot GetPrevious(int index)
        {
            TSlot slot = Deferred?.Get<This>()?.GetSlot(index);
            slot?.SetLastUser(this);
            return slot;
        }

        private List<int> SlotsMask
            => slotsMask ??= Enumerable.Range((int)StartSlot, (int)NumSlots).Select(i => i).ToList();

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
            Slots.Where(s => s.Index >= other.StartSlot && s.Index < other.StartSlot + other.NumSlots)
                .ToList().ForEach(s => Slots.Remove(s));

            PointersMerged ??= new List<ulong> { Pointer };
            PointersMerged.Add(other.Pointer);
        }
    }
}
