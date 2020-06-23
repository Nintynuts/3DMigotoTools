namespace Migoto.Log.Parser.Slot
{
    public class Resource : Base, ISlotResource
    {
        public ulong Pointer { get; set; }

        public Asset.Base Asset { get; set; }

        public void UpdateAsset(Asset.Base asset)
        {
            Asset?.Unregister(this);
            Asset = asset;
            if (Owner != null)
                Asset?.Register(this);
        }

        public override void SetOwner(DriverCall.Base newOwner)
        {
            if (newOwner == null && Owner != null)
                Asset?.Unregister(this);
            if (Owner == null && newOwner != null)
                Asset?.Register(this);
            Owner = newOwner;
        }
    }
}
