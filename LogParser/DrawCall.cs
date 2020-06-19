
using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.DriverCall.Draw;

namespace Migoto.Log.Parser
{

    public class DrawCall : IDeferred<DrawCall>
    {
        public Deferred<DrawCall> Deferred { get; }

        private readonly Dictionary<ShaderType, ShaderContext> lookup;

        public DrawCall(uint index, DrawCall previous)
        {
            Index = index;
            Deferred = new Deferred<DrawCall>(previous);
            lookup = Enums.Values<ShaderType>().ToDictionary(s => s, s => new ShaderContext(s, this, previous));
        }
        public uint Index { get; }

        public string Logic { get; set; }

        public IDraw Draw { get; set; }

        public List<Map> Mappings { get; } = new List<Map>();
        public List<Unmap> Unmappings { get; } = new List<Unmap>();

        public List<CopyResource> ResourceCopied { get; } = new List<CopyResource>();
        public List<CopySubresourceRegion> SubresourceRegionCopied { get; } = new List<CopySubresourceRegion>();
        public List<ClearDepthStencilView> DepthStencilCleared { get; } = new List<ClearDepthStencilView>();
        public List<ClearRenderTargetView> RenderTargetCleared { get; } = new List<ClearRenderTargetView>();

        public RSSetState RasterizerState { get => Deferred.Get<RSSetState>(); set => Deferred.Set(value); }
        public List<RSSetViewports> Viewports { get; } = new List<RSSetViewports>();

        public OMSetRenderTargets SetRenderTargets { get => Deferred.Get<OMSetRenderTargets>(); set => Deferred.Set(value); }
        public OMSetBlendState BlendState { get => Deferred.Get<OMSetBlendState>(); set => Deferred.Set(value); }
        public OMSetDepthStencilState DepthStencilState { get => Deferred.Get<OMSetDepthStencilState>(); set => Deferred.Set(value); }
        public OMGetRenderTargetsAndUnorderedAccessViews GetRTsAndUAVs { get => Deferred.Get<OMGetRenderTargetsAndUnorderedAccessViews>(false); set => Deferred.Set(value); }

        public IASetPrimitiveTopology PrimitiveTopology { get => Deferred.Get<IASetPrimitiveTopology>(); set => Deferred.Set(value); }

        public IASetInputLayout InputLayout { get => Deferred.Get<IASetInputLayout>(); set => Deferred.Set(value); }

        public List<IASetVertexBuffers> SetVertexBuffers { get; } = new List<IASetVertexBuffers>();

        public List<IASetIndexBuffer> SetIndexBuffer { get; } = new List<IASetIndexBuffer>();

        public ShaderContext Shader(ShaderType type) => lookup[type];
    }
}
