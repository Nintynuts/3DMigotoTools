
using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Parser.Slot
{
    public class Sampler : Base, IOwned<DriverCall.Base>
    {
        public Sampler(DriverCall.Base owner) : base(owner) { }

        public ulong Handle { get; set; }
    }
}
