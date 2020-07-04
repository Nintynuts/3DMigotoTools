
namespace Migoto.Log.Parser.ApiCalls
{
    using Assets;
    using Slots;

    public interface ISingleSlot
    {
        IResource Slot { get; }
    }

    public abstract class SingleSlot : ApiCall, ISingleSlot, IResource
    {
        protected SingleSlot(uint order) : base(order) { }

        public Asset Asset { get; protected set; }

        IApiCall IResource.Owner => this;

        ulong IResource.Pointer => Pointer;
        protected ulong Pointer { get; set; }

        IResource ISingleSlot.Slot => this;

        public void UpdateAsset(Asset asset)
        {
            Asset?.Unregister(this);
            Asset = asset;
            Asset?.Register(this);
        }
    }
}
