
using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.DriverCall;

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
            PixelShader = new ShaderContext(ShaderType.Pixel, this, previous?.PixelShader);
            VertexShader = new ShaderContext(ShaderType.Vertex, this, previous?.VertexShader);
            ComputeShader = new ShaderContext(ShaderType.Compute, this, previous?.ComputeShader);
            DomainShader = new ShaderContext(ShaderType.Domain, this, previous?.DomainShader);
            HullShader = new ShaderContext(ShaderType.Hull, this, previous?.HullShader);
            GeometryShader = new ShaderContext(ShaderType.Geometry, this, previous?.GeometryShader);

            lookup = new[] { PixelShader, VertexShader, ComputeShader, DomainShader, HullShader, GeometryShader }.ToDictionary(s => s.ShaderType, s => s);
        }
        public uint Index { get; }

        public string Logic { get; set; }

        public Draw Draw { get; set; }
        public DrawIndexed DrawIndexed { get; set; }

        public List<Map> Mappings { get; } = new List<Map>();
        public List<Unmap> Unmappings { get; } = new List<Unmap>();

        public CopySubresourceRegion SubresourceRegionCopied { get => Deferred.Get<CopySubresourceRegion>(false); set => Deferred.Set(value); }
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

        public ShaderContext PixelShader { get; set; }
        public ShaderContext VertexShader { get; set; }
        public ShaderContext ComputeShader { get; set; }
        public ShaderContext DomainShader { get; set; }
        public ShaderContext HullShader { get; set; }
        public ShaderContext GeometryShader { get; set; }

        public ShaderContext Shader(ShaderType type) => lookup[type];
    }
}
