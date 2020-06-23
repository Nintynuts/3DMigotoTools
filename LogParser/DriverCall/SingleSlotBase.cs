
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.DriverCall
{
    public abstract class SingleSlotBase : Base, IResource, ISingleSlot
    {
        protected SingleSlotBase(uint order) : base(order) { }

        public Asset.Base Asset { get; protected set; }

        Base IResource.Owner => this;

        ulong IResource.Pointer => Pointer;
        protected ulong Pointer { get; set; }

        IResource ISingleSlot.Target => this;

        public void UpdateAsset(Asset.Base asset)
        {
            Asset?.Unregister(this);
            Asset = asset;
            Asset?.Register(this);
        }
    }
}
