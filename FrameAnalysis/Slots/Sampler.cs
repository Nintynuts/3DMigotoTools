namespace Migoto.Log.Parser.Slots;

using ApiCalls;

public class Sampler : Slot<IApiCall>
{
    public ulong Handle { get; set; }
}
