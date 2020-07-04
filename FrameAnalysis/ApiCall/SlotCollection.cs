namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    internal class SlotCollection<TOwner, TSlot, TSlotOwner> : OwnedCollection<TSlotOwner, TSlot>
        where TOwner : class, IMultiSlot, TSlotOwner
        where TSlot : ISlot<TSlotOwner>, IOwned<TSlotOwner>
        where TSlotOwner : class
    {
        private readonly TOwner owner;

        public SlotCollection(TOwner owner) : base(owner)
        {
            this.owner = owner;
        }

        public override void Add(TSlot item)
        {
            if (item == null)
                return;
            base.Add(item);
            if (!owner.GlobalSlotsMask.Contains(item.Index))
                owner.GlobalSlotsMask.Add(item.Index);
        }
    }
}
