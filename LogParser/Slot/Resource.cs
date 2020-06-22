namespace Migoto.Log.Parser.Slot
{
    public class Resource : Base, ISlotResource
    {
        public Resource(DriverCall.Base owner) : base(owner) { }

        public ulong Pointer { get; set; }

        public Asset.Base Asset { get; set; }

        public void UpdateAsset(Asset.Base asset) => Asset = asset;

        public override void SetOwner(DriverCall.Base newOwner)
        {
            if (Owner != null)
                Asset?.Uses.Remove(this);
            Owner = newOwner;
            if (Owner != null)
                Asset?.Uses.Add(this);
        }
    }
}
