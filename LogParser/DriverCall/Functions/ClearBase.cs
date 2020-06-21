
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public abstract class ClearBase : Base
    {
        protected ClearBase(uint order, DrawCall owner) : base(order, owner) { }

        public ResourceView ResourceView { get; set; }
    }
}
