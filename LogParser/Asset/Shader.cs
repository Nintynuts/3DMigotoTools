using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Parser.Asset
{
    public class Shader
    {
        public Shader(ShaderType shaderType)
        {
            ShaderType = shaderType;
        }

        public ShaderType ShaderType { get; set; }

        public List<DrawCall> References { get; } = new List<DrawCall>();

        [TypeConverter(typeof(LongHashTypeConverter))]
        public ulong Hash { get; set; }

        public ICollection<Shader> PartnerVS => References.Select(r => r.VertexShader).Consolidate();
        public ICollection<Shader> PartnerPS => References.Select(r => r.PixelShader).Consolidate();
        public ICollection<Shader> PartnerHS => References.Select(r => r.HullShader).Consolidate();
        public ICollection<Shader> PartnerDS => References.Select(r => r.DomainShader).Consolidate();
        public ICollection<Shader> PartnerGS => References.Select(r => r.GeometryShader).Consolidate();

        public ICollection<Texture> PartnerTextures => PartnerResource<Texture>(ctx => ctx.SetShaderResources?.ResourceViews);
        public ICollection<Texture> PartnerRTs => PartnerResource<Texture>(ctx => ctx.Owner.SetRenderTargets.RenderTargets);
        public ICollection<Buffer> PartnerBuffers => PartnerResource<Buffer>(ctx => ctx.SetConstantBuffers?.ConstantBuffers);

        private ICollection<T> PartnerResource<T>(System.Func<ShaderContext, IEnumerable<Resource>> selector)
            => References.SelectMany(r => selector(r.Shader(ShaderType))?.Select(rv => rv.Asset).OfType<T>() ?? Enumerable.Empty<T>()).Consolidate();
    }

    internal static class ShaderExtentions
    {

        public static ICollection<T> Consolidate<T>(this IEnumerable<T> items) =>
            items.Where(s => s != null).Distinct().ToList();

        public static ICollection<Shader> Consolidate(this IEnumerable<ShaderContext> context)
            => context.Select(c => c.SetShader?.Shader).Consolidate();
    }
}
