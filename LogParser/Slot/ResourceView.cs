namespace Migoto.Log.Parser.Slot
{
    public class ResourceView : Resource
    {
        public ResourceView(DriverCall.Base owner) : base(owner) { }

        public ulong View { get; set; }
    }
}
