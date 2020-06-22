using System.Linq;

using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.Asset
{
    public class Texture : Base
    {
        public bool IsRenderTarget => Uses.OfType<ISlotResource>().Any(s => s.Owner is OMSetRenderTargets && s.Index >= 0);
        public bool IsDepthStencil => Uses.OfType<ISlotResource>().Any(s => s.Owner is OMSetRenderTargets && s.Index == -1);
    }
}
