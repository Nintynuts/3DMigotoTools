
namespace Migoto.Log.Parser.Slots
{
    using ApiCalls;
    using Assets;

    public class Resource : Slot<IApiCall>, IResourceSlot
    {
        public ulong Pointer { get; set; }

        public Asset Asset { get; set; }

        public void UpdateAsset(Asset asset)
        {
            Asset?.Unregister(this);
            Asset = asset;
            if (Owner != null)
                Asset?.Register(this);
        }

        public override void SetOwner(IApiCall newOwner)
        {
            if (newOwner == null && Owner != null)
                Asset?.Unregister(this);
            if (Owner == null && newOwner != null)
                Asset?.Register(this);
            Owner = newOwner;
        }
    }
}
