using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.DriverCall
{
    public interface ISlotsUsage
    {
        List<int> SlotsUsed { get; }
    }

    public abstract class SlotsBase<This, TSlotType, TDeferred> : Base, IMergable<This>, ISlotsUsage
        where This : SlotsBase<This, TSlotType, TDeferred>
        where TSlotType : Slot.Base
        where TDeferred : class, IDeferred<TDeferred, DrawCall>
    {
        private readonly List<string> mergeWarnings = new List<string>();

        private List<int> slotsMask;
        private List<TSlotType> allSlots;

        protected SlotsBase(uint order, DrawCall owner) : base(order, owner)
        {
            Slots = new SlotCollection<SlotsBase<This, TSlotType, TDeferred>, TSlotType>(this);
        }

        public abstract List<int> SlotsUsed { get; }

        protected abstract Deferred<TDeferred, DrawCall> Deferred { get; }

        protected virtual string Name => GetType().Name;

        protected ICollection<TSlotType> Slots { get; }
        public uint StartSlot { get; set; }
        protected uint NumSlots { get; set; }
        protected ulong Pointer { get; set; }
        public List<ulong> PointersMerged { get; protected set; }

        protected List<TSlotType> AllSlots => allSlots ??= SlotsUsed.OrderBy(i => i).Select(GetSlot).ToList();

        private TSlotType GetSlot(int index)
            => SlotsMask.Contains(index) ? Slots.FirstOrDefault(s => s.Index == index) : Deferred?.Get<This>()?.GetSlot(index);

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
            Slots.Where(s => s.Index >= other.StartSlot && s.Index < other.StartSlot + other.NumSlots).ToList().ForEach(s =>
            {
                s.SetOwner(null);
                Slots.Remove(s);
            });
            PointersMerged ??= new List<ulong> { Pointer };
            PointersMerged.Add(other.Pointer);
        }
    }
}
