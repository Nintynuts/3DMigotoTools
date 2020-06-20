namespace Migoto.Log.Parser.Slot
{
    public class Resource : IResource, IOwned<DriverCall.Base>
    {
        public ulong Pointer { get; set; }

        public int Index { get; set; } = -1;

        public Asset.Base Asset { get; set; }

        public DriverCall.Base Owner { get; private set; }

        public void SetOwner(DriverCall.Base newOwner)
        {
            if (Owner != null)
                Asset.Uses.Remove(this);
            Owner = newOwner;
            if (Owner != null)
                Asset.Uses.Add(this);
        }

        public Resource(DriverCall.Base owner)
        {
            Owner = owner;
        }

        public void UpdateAsset(Asset.Base asset) => Asset = asset;
    }
}
