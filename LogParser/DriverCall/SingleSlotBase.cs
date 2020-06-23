
namespace Migoto.Log.Parser.ApiCalls
{
    using Assets;
    using Slots;

    public interface ISingleSlot
    {
        IResource Target { get; }
    }

    public abstract class SingleSlot : ApiCall, IResource, ISingleSlot
    {
        protected SingleSlot(uint order) : base(order) { }

        public Asset Asset { get; protected set; }

        ApiCall IResource.Owner => this;

        ulong IResource.Pointer => Pointer;
        protected ulong Pointer { get; set; }

        IResource ISingleSlot.Target => this;

        public void UpdateAsset(Asset asset)
        {
            Asset?.Unregister(this);
            Asset = asset;
            Asset?.Register(this);
        }
    }
}
