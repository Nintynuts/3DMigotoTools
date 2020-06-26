using System.Collections.Generic;

namespace Migoto.Log.Parser.ApiCalls
{
    using Slots;

    public abstract class MultiSlot<This, TSlot> : MultiSlotBase<This, TSlot, IApiCall, DrawCall, DrawCall>, IApiCall
        where This : MultiSlotBase<This, TSlot, IApiCall, DrawCall, DrawCall>, IApiCall
        where TSlot : class, ISlot<IApiCall>, IOwned<IApiCall>
    {
        public static List<int> UsedSlots { get; } = new List<int>();

        protected MultiSlot(uint order) => Order = order;

        public uint Order { get; }

        public override List<int> GlobalSlotsMask => UsedSlots;

        protected override Deferred<DrawCall, DrawCall> PreviousDeferred => Owner.Fallback.Deferred;
    }
}
