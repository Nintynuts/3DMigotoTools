using System.Collections.Generic;

namespace Migoto.Log.Parser.DriverCall
{
    public abstract class Slots<This, TSlotType> : SlotsBase<This, TSlotType, DrawCall>
        where This : SlotsBase<This, TSlotType, DrawCall>
        where TSlotType : Slot.Base
    {
        public static List<int> UsedSlots { get; } = new List<int>();

        protected Slots(uint order) : base(order) { }

        public override List<int> SlotsUsed => UsedSlots;

        protected override Deferred<DrawCall, DrawCall> Deferred => Owner.Deferred;
    }
}
