using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Migoto.Log.Parser.Assets
{
    using ApiCalls;
    using Slots;
    using static ApiCalls.ShaderType;

    public class Shader : IHash
    {
        public Shader(ShaderType shaderType)
        {
            ShaderType = shaderType;
        }

        public ShaderType ShaderType { get; set; }

        public List<DrawCall> References { get; } = new List<DrawCall>();

        [TypeConverter(typeof(LongHashTypeConverter))]
        public ulong Hash { get; set; }

        public string Hex => $"{Hash:X16}";

        private ICollection<Shader> Partner(ShaderType type) => References.Select(r => r.Shader(type)).Select(c => c.SetShader?.Shader).Consolidate();
        public ICollection<Shader> PartnerVS => Partner(Vertex);
        public ICollection<Shader> PartnerPS => Partner(Pixel);
        public ICollection<Shader> PartnerHS => Partner(Hull);
        public ICollection<Shader> PartnerDS => Partner(Domain);
        public ICollection<Shader> PartnerGS => Partner(Geometry);

        public ICollection<Texture> PartnerTextures => PartnerResource<Texture>(ctx => ctx.SetShaderResources?.ResourceViews);
        public ICollection<Texture> PartnerRTs => PartnerResource<Texture>(ctx => ctx.Owner.SetRenderTargets?.RenderTargets);
        public ICollection<Buffer> PartnerBuffers => PartnerResource<Buffer>(ctx => ctx.SetConstantBuffers?.ConstantBuffers);

        private ICollection<T> PartnerResource<T>(Func<ShaderContext, IEnumerable<Resource>> selector)
            => References.SelectMany(r => selector(r.Shader(ShaderType))?.Select(rv => rv.Asset).OfType<T>() ?? Enumerable.Empty<T>()).Consolidate();
    }
}
