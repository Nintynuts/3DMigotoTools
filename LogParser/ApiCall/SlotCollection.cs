namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    internal class SlotCollection<TSlotsApiCall, TSlot> : OwnedCollection<ApiCall, TSlot>
        where TSlotsApiCall : ApiCall, IMultiSlot
        where TSlot : Slot
    {
        private readonly TSlotsApiCall owner;

        public SlotCollection(TSlotsApiCall owner) : base(owner)
        {
            this.owner = owner;
        }

        public override void Add(TSlot item)
        {
            if (item == null)
                return;
            base.Add(item);
            if (!owner.SlotsUsed.Contains(item.Index))
                owner.SlotsUsed.Add(item.Index);
        }
    }
}
