using System.Collections.Generic;

namespace Migoto.Log.Parser.DriverCall
{
    interface IMergableSlots<TDriverCall, TSlotType> : IMergable<TDriverCall> where TSlotType : IOwned<Base>
    {
        List<TSlotType> Slots { get; }

        uint StartSlot { get; set; }
        uint NumSlots { get; set; }
        ulong Pointer { get; }

        List<ulong> PointersMerged { get; set; }

        public void DoMerge(IMergableSlots<TDriverCall, TSlotType> value)
        {
            Slots.AddRange(value.Slots);
            value.Slots.ForEach(s => s.SetOwner((Base)this));
            var originalStart = StartSlot;
            if (StartSlot > value.StartSlot)
                StartSlot = value.StartSlot;
            if (originalStart + NumSlots < value.StartSlot + value.NumSlots)
                NumSlots = value.NumSlots;

            PointersMerged ??= new List<ulong>();
            PointersMerged.Add(value.Pointer);
        }
    }
}
