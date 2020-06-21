
namespace Migoto.Log.Parser.DriverCall
{
    internal class SlotCollection<TSlotDriverCall, TSlotType> : OwnedCollection<Base, TSlotType>
        where TSlotDriverCall : Base, ISlotsUsage
        where TSlotType : Slot.Base
    {
        private readonly TSlotDriverCall owner;

        public SlotCollection(TSlotDriverCall owner) : base(owner)
        {
            this.owner = owner;
        }

        public override void Add(TSlotType item)
        {
            if (item == null)
                return;
            base.Add(item);
            if (!owner.SlotsUsed.Contains(item.Index))
                owner.SlotsUsed.Add(item.Index);
        }
    }
}
