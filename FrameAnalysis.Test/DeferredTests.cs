using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Migoto.Log.Parser.Test
{
    [TestClass]
    public class DeferredTests
    {
        [TestMethod]
        public void TestDeferred()
        {
            var first = new TestThing();
            var second = new TestThing(first);

            var firstOwnedThing = new OwnedThing();
            first.TestProp = firstOwnedThing;

            Assert.AreEqual(firstOwnedThing, first.TestProp, "First's thing can't be retrieved");

            Assert.AreEqual(firstOwnedThing, second.TestProp, "Second doesn't inherit property from first");

            Assert.AreEqual(first, firstOwnedThing.Owner, "First doesn't own its own thing");

            Assert.AreEqual(first, second.TestProp?.Owner, "First doesn't own second's thing");

            var secondOwnedThing = new OwnedThing();
            first.TestProp = secondOwnedThing;

            Assert.AreEqual(secondOwnedThing, first.TestProp, "First's thing hasn't been replaced");

            Assert.AreEqual(1, first.Deferred.Collisions.Count(), "Replacing the thing wasn't logged");

            second.AnotherProp = firstOwnedThing;

            Assert.AreEqual(firstOwnedThing, second.AnotherProp, "Second's thing can't be retrieved");

            Assert.AreEqual(second, firstOwnedThing.Owner, "Owner of thing wasn't updated to second");

            first.NotInterited = new MergableThing();
            first.NotInterited = new MergableThing();

            Assert.IsTrue(first.NotInterited.Merged, "First's merge prop not marked as merged");
        }

        [TestMethod]
        public void TestDeferredMultiLayer()
        {
            var first = new TestThing();
            var second = new TestThing(first);
            var third = new TestThing(second);

            var firstOwnedThing = new OwnedThing();
            first.TestProp = firstOwnedThing;

            Assert.AreEqual(firstOwnedThing, third.TestProp, "Third's thing can't be retrieved");

            Assert.AreEqual(third, first.TestProp.LastUser, "Third not registered as last user");

            first.NotInterited = new MergableThing();

            Assert.IsNull(third.NotInterited, "Third should not inherit First's AnotherProp value");
        }
    }

    internal class OwnedThing : IOwned<TestThing>, IOverriden<TestThing>
    {
        public TestThing? Owner { get; private set; }

        public TestThing? LastUser { get; private set; }

        public void SetLastUser(TestThing lastUser) => LastUser = lastUser;

        public void SetOwner(TestThing? newOwner) => Owner = newOwner;
    }

    internal class MergableThing : OwnedThing, IMergable<MergableThing>
    {
        public IEnumerable<string> MergeWarnings { get; } = new List<string>();

        public bool Merged { get; set; }

        public void Merge(MergableThing value) => Merged = true;
    }

    internal class TestThing : IDeferred<TestThing, TestThing>
    {
        public TestThing(TestThing? fallback = null)
        {
            Deferred = new Deferred<TestThing, TestThing>(this, fallback);
            Fallback = fallback;
        }

        public TestThing? Fallback { get; }

        public Deferred<TestThing, TestThing> Deferred { get; }

        public OwnedThing? TestProp { get => Deferred.Get<OwnedThing>(); set => Deferred.Set(value); }

        public OwnedThing? AnotherProp { get => Deferred.Get<OwnedThing>(); set => Deferred.Set(value); }

        public MergableThing? NotInterited { get => Deferred.Get<MergableThing>(false); set => Deferred.Set(value); }
    }
}
