using System.Linq;

namespace Migoto.Log.Parser.Assets
{
    using ApiCalls;
    using Slots;

    public class Texture : Asset
    {
        public bool IsRenderTarget => Uses.OfType<IResourceSlot>().Any(s => s.Owner is OMSetRenderTargets && s.Index >= 0);
        public bool IsDepthStencil => Uses.OfType<IResourceSlot>().Any(s => s.Owner is OMSetRenderTargets && s.Index == -1);
    }
}
