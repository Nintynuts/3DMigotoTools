using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public interface IMultiSlot
    {
        List<int> GlobalSlotsMask { get; }

        IEnumerable<IResourceSlot> Slots { get; }

        uint StartSlot { get; }
        uint NumSlots { get; }
        ulong Pointer { get; }
        List<ulong>? PointersMerged { get; }
    }

    public abstract class MultiSlotBase<This, TSlot, TSlotOwner, TOwner, TFallback> : IOwned<TOwner>, IOverriden<TOwner>, IMergable<This>, IMultiSlot, INamed
        where This : MultiSlotBase<This, TSlot, TSlotOwner, TOwner, TFallback>, TSlotOwner
        where TSlot : class, ISlot<TSlotOwner>, IOwned<TSlotOwner>
        where TSlotOwner : class
        where TOwner : class
        where TFallback : class, IDeferred<TFallback, TOwner>
    {
        private readonly List<string> mergeWarnings = new List<string>();

        private List<int>? slotsMask;
        private List<TSlot>? slotsSet;

        protected MultiSlotBase()
        {
            SlotsPopulated = new SlotCollection<This, TSlot, TSlotOwner>((This)this);
        }

        public uint StartSlot { get; set; }

        protected uint NumSlots { get; set; }
        uint IMultiSlot.NumSlots => NumSlots;

        protected ulong Pointer { get; set; }
        ulong IMultiSlot.Pointer => Pointer;

        public List<ulong>? PointersMerged { get; protected set; }
        List<ulong>? IMultiSlot.PointersMerged => PointersMerged;

        protected ICollection<TSlot> SlotsPopulated { get; }

        private List<int> SlotsMask
            => slotsMask ??= Enumerable.Range((int)StartSlot, (int)NumSlots).ToList();

        protected List<TSlot> SlotsSet
            => slotsSet ??= GlobalSlotsMask.OrderBy(i => i).Select(GetSlot).ExceptNull().ToList();

        public abstract List<int> GlobalSlotsMask { get; }

        protected abstract Deferred<TFallback, TOwner>? PreviousDeferred { get; }

        IEnumerable<IResourceSlot> IMultiSlot.Slots => SlotsSet.Cast<IResourceSlot>();

        public TOwner? Owner { get; private set; }

        public void SetOwner(TOwner? newOwner) => Owner = newOwner;

        public TOwner? LastUser { get; private set; }

        public void SetLastUser(TOwner lastUser) => LastUser = lastUser;

        private TSlot? GetSlot(int index)
            => SlotsMask.Contains(index) ? SlotsPopulated.FirstOrDefault(s => s.Index == index) : GetPrevious(index);

        private TSlot? GetPrevious(int index)
        {
            var slot = PreviousDeferred?.OfType<This>().FirstOrDefault()?.SlotsSet.Find(s => s?.Index == index);
            slot?.SetLastUser((This)this);
            return slot;
        }

        public IEnumerable<string> MergeWarnings => mergeWarnings;

        public virtual void Merge(This other)
        {
            for (uint i = 0; i < other.NumSlots; i++)
            {
                var slotIdx = (int)(other.StartSlot + i);
                if (!SlotsMask.Contains(slotIdx))
                    SlotsMask.Add(slotIdx);
            }

            var overridden = SlotsPopulated.Where(s => s.Index >= other.StartSlot && s.Index < other.StartSlot + other.NumSlots).ToList();
            overridden.ForEach(s => mergeWarnings.Add($"{(this as INamed).Name}: Overwriting slot {s.Index}"));
            overridden.ForEach(s => SlotsPopulated.Remove(s));

            NumSlots = (uint)SlotsMask.Count;
            StartSlot = other.StartSlot < StartSlot ? other.StartSlot : StartSlot;
            PointersMerged ??= new List<ulong> { Pointer };
            PointersMerged.Add(other.Pointer);
        }
    }
}
