using System.Linq;

using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Parser.Asset
{
    public class Texture : Base
    {
        public bool IsRenderTarget => Uses.Any(s => s.Owner is OMSetRenderTargets && s.Index >= 0);
        public bool IsDepthStencil => Uses.Any(s => s.Owner is OMSetRenderTargets && s.Index == -1);
    }
}
