using Migoto.Log.Parser.ApiCalls;

namespace Migoto.Log.Parser.Slots
{
    public class Sampler : Slot<IApiCall>
    {
        public ulong Handle { get; set; }
    }
}
