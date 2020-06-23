using System.Collections.Generic;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public abstract class MultiSlot<This, TSlotType> : MultiSlotBase<This, TSlotType, DrawCall>
        where This : MultiSlotBase<This, TSlotType, DrawCall>
        where TSlotType : Slot
    {
        public static List<int> UsedSlots { get; } = new List<int>();

        protected MultiSlot(uint order) : base(order) { }

        public override List<int> SlotsUsed => UsedSlots;

        protected override Deferred<DrawCall, DrawCall> Deferred => Owner.Deferred;
    }
}
