
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public abstract class ClearBase : Base, ISingleSlot
    {
        protected ClearBase(uint order) : base(order) { }

        public ResourceView Target { get; set; }

        IResource ISingleSlot.Target => Target;
    }
}
