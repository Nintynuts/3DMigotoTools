using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.Assets
{
    using ApiCalls;

    using Slots;

    public class Texture : Asset
    {
        private readonly List<IResourceSlot> outputSlots = new();

        public bool IsRenderTarget => outputSlots.Any(s => s.Index >= 0);
        public bool IsDepthStencil => outputSlots.Any(s => s.Index == -1);


        public override void Register(IResource resource)
        {
            base.Register(resource);
            if (resource is IResourceSlot { Owner: OMSetRenderTargets } outputSlot)
                outputSlots.Add(outputSlot);
        }

        public override void Unregister(IResource resource)
        {
            base.Unregister(resource);
            if (resource is IResourceSlot { Owner: OMSetRenderTargets } outputSlot)
                outputSlots.Remove(outputSlot);
        }
    }
}
