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

            Assert.AreEqual(first, second.TestProp.Owner, "First doesn't own second's thing");

            var secondOwnedThing = new OwnedThing();
            first.TestProp = secondOwnedThing;

            Assert.AreEqual(secondOwnedThing, first.TestProp, "First's thing hasn't been replaced");

            Assert.AreEqual(1, first.Deferred.Collisions.Count(), "Replacing the thing wasn't logged");

            second.AnotherProp = firstOwnedThing;

            Assert.AreEqual(firstOwnedThing, second.AnotherProp, "Second's thing can't be retrieved");

            Assert.AreEqual(second, firstOwnedThing.Owner, "Owner of thing wasn't updated to second");
        }
    }

    internal class OwnedThing : IOwned<TestThing>
    {
        public TestThing Owner { get; private set; }

        public void SetOwner(TestThing newOwner) { Owner = newOwner; }
    }


    internal class TestThing : IDeferred<TestThing, TestThing>
    {
        public TestThing(TestThing fallback = null)
        {
            Deferred = new Deferred<TestThing, TestThing>(this, fallback);
        }

        public Deferred<TestThing, TestThing> Deferred { get; }

        public OwnedThing TestProp { get => Deferred.Get<OwnedThing>(); set => Deferred.Set(value); }

        public OwnedThing AnotherProp { get => Deferred.Get<OwnedThing>(); set => Deferred.Set(value); }
    }
}
