
using System.Collections.Generic;
using System.Linq;

using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Parser
{

    public class DrawCall
    {
        private Deferred<DrawCall> props;

        private Dictionary<ShaderType, ShaderContext> lookup;

        public DrawCall(uint index, DrawCall previous)
        {
            Index = index;
            props = new Deferred<DrawCall>(previous);
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

        public CopySubresourceRegion SubresourceRegionCopied { get => props.Get<CopySubresourceRegion>(false); set => props.Set(value); }
        public List<ClearDepthStencilView> DepthStencilCleared { get; } = new List<ClearDepthStencilView>();
        public List<ClearRenderTargetView> RenderTargetCleared { get; } = new List<ClearRenderTargetView>();

        public RSSetState RasterizerState { get => props.Get<RSSetState>(); set => props.Set(value); }
        public List<RSSetViewports> Viewports { get; } = new List<RSSetViewports>();

        public OMSetRenderTargets SetRenderTargets { get => props.Get<OMSetRenderTargets>(); set => props.Set(value); }
        public OMSetBlendState BlendState { get => props.Get<OMSetBlendState>(); set => props.Set(value); }
        public OMSetDepthStencilState DepthStencilState { get => props.Get<OMSetDepthStencilState>(); set => props.Set(value); }
        public OMGetRenderTargetsAndUnorderedAccessViews GetRTsAndUAVs { get => props.Get<OMGetRenderTargetsAndUnorderedAccessViews>(false); set => props.Set(value); }

        public IASetPrimitiveTopology PrimitiveTopology { get => props.Get<IASetPrimitiveTopology>(); set => props.Set(value); }

        public IASetInputLayout InputLayout { get => props.Get<IASetInputLayout>(); set => props.Set(value); }

        public List<IASetVertexBuffers> VertexBuffers { get; } = new List<IASetVertexBuffers>();

        public List<IASetIndexBuffer> IndexBuffer { get; } = new List<IASetIndexBuffer>();

        public ShaderContext PixelShader { get; set; }
        public ShaderContext VertexShader { get; set; }
        public ShaderContext ComputeShader { get; set; }
        public ShaderContext DomainShader { get; set; }
        public ShaderContext HullShader { get; set; }
        public ShaderContext GeometryShader { get; set; }

        public ShaderContext Shader(ShaderType type) => lookup[type];
    }
}
