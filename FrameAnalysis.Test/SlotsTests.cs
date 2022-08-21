using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Migoto.Log.Parser.Test
{

    using ApiCalls;

    using Slots;

    [TestClass]
    public class SlotsTests
    {
        [TestMethod]
        public void SlotsMerge()
        {
            var multiSlot1 = new TestSlots(0, 5);
            var multiSlot2 = new TestSlots(3, 5);

            multiSlot1.Merge(multiSlot2);

            IMultiSlot<TestSlot> multiSlot = multiSlot1;
            Assert.AreEqual(0u, multiSlot.StartSlot);
            Assert.AreEqual(8u, multiSlot.NumSlots);
            Assert.AreEqual(2, multiSlot1.PointersMerged?.Count);
            Assert.AreEqual(2, multiSlot1.MergeWarnings.Count());
        }

        [TestMethod]
        public void SparseSlotsFallback()
        {
            var deferred1 = new TestDeferred();
            var deferred2 = new TestDeferred(deferred1);

            deferred1.TestSlots = new TestSlots(0, 10, i => i % 2 == 0);
            deferred2.TestSlots = new TestSlots(0, 10, i => i % 2 == 1, setByMerge: true);

            Assert.AreEqual(10, deferred2.TestSlots.GlobalSlotsMask.Count, "Global Slots Mask");
            Assert.AreEqual(10, deferred2.TestSlots.Slots.Count(s => s != null), "Not Null Slots");
        }

        [TestMethod]
        public void NullSlotsDoNotFallback()
        {
            var deferred1 = new TestDeferred();
            var deferred2 = new TestDeferred(deferred1);

            deferred1.TestSlots = new TestSlots(0, 10, i => i % 2 == 0);
            deferred2.TestSlots = new TestSlots(0, 10, i => i % 2 == 1);

            Assert.AreEqual(10, deferred2.TestSlots.GlobalSlotsMask.Count, "Global Slots Mask");
            Assert.AreEqual(10, deferred2.TestSlots.Slots.Count(), "Set Slots");
            Assert.AreEqual(5, deferred2.TestSlots.Slots.Count(s => s != null), "Not Null Slots");
        }
    }

    internal class TestDeferred : IDeferred<TestDeferred, TestDeferred>
    {
        public TestDeferred(TestDeferred? fallback = null)
        {
            Deferred = new Deferred<TestDeferred, TestDeferred>(this, fallback);
            Fallback = fallback;
        }

        public TestDeferred? Fallback { get; }

        public Deferred<TestDeferred, TestDeferred> Deferred { get; }

        public TestSlots? TestSlots { get => Deferred.Get<TestSlots>(); set => Deferred.Set(value); }
    }

    internal class TestSlots : MultiSlotBase<TestSlots, TestSlot, TestSlots, TestDeferred, TestDeferred>
    {
        public TestSlots(uint startSlot, uint numSlots, Func<int, bool>? pattern = null, bool setByMerge = false)
        {
            StartSlot = startSlot;
            NumSlots = setByMerge ? 0 : numSlots;
            if (setByMerge)
                InitData(i => new TestSlots((uint)i, 1), Merge);

            InitData(i => new TestSlot(i), SlotsPopulated.Add);

            void InitData<T>(Func<int, T> construct, Action<T> method) where T : class
            {
                Enumerable.Range((int)StartSlot, (int)numSlots)
                    .Select(i => pattern?.Invoke(i) != false ? construct(i) : null)
                    .ExceptNull().ForEach(method);
            }
        }


        public IEnumerable<TestSlot?> Slots => SlotsSet;

        private static readonly List<int> globalSlotsMask = new List<int>();

        public override List<int> GlobalSlotsMask => globalSlotsMask;

        protected override Deferred<TestDeferred, TestDeferred>? PreviousDeferred => Owner?.Fallback?.Deferred;
    }

    internal class TestSlot : Slot<TestSlots>
    {
        public TestSlot(int index)
        {
            Index = index;
        }
    }
}
