namespace Migoto.Log.Parser.Slots
{
    using ApiCalls;

    public class Sampler : Slot, IOwned<ApiCall>
    {
        public ulong Handle { get; set; }
    }
}
