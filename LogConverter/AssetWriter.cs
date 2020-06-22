using System.IO;
using System.Linq;

using Migoto.Log.Parser;
using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.Slot;

namespace Migoto.Log.Converter
{
    using Column = Column<Parser.DriverCall.Base>;
    using HashColumn = HashColumn<Parser.DriverCall.Base>;
    using IColumns = IColumns<Parser.DriverCall.Base>;
    internal class AssetWriter
    {
        public static void Write(Parser.Asset.Base asset, StreamWriter output)
        {
            var columns = new IColumns[]
            {
                new Column("FirstDrawCall", dc => dc.Owner.Index),
                new Column("LastDrawCall", dc => (((dc is IResourceSlots resourceSlots ? GetResource(asset, resourceSlots).LastUser?.Owner : null) ?? dc.LastUser)?.Index)),
                new Column("DriverCall", dc => dc.Name),
                new Column("Slot", dc => dc is IResourceSlots resourceSlots ? (object)GetResource(asset, resourceSlots).Index : GetName(asset, dc)),
                new HashColumn("Shader", dc => GetShader(dc)?.SetShader.Shader),
            };

            output.WriteLine($"Asset is a {asset.GetType().Name},{GetAssetSubType(asset)}");
            output.WriteLine(columns.Headers());
            asset.LifeCycle.ForEach(dc => output.WriteLine(columns.Values(dc)));
            output.Close();
        }

        private static ShaderContext GetShader(Parser.DriverCall.Base dc)
        {
            return dc is IShaderCall shaderCall ? dc.Owner.Shader(shaderCall.ShaderType)
                 : dc is IInputAssembler ? dc.Owner.Shader(ShaderType.Vertex)
                 : dc is IOutputMerger ? dc.Owner.Shader(ShaderType.Pixel)
                 : null;
        }

        private static string GetAssetSubType(Parser.Asset.Base asset)
        {
            return asset is Texture texture
                     ? texture.IsRenderTarget ? "Render Target"
                     : texture.IsDepthStencil ? "Depth Stencil"
                     : string.Empty
                 : asset is ConstantBuffer cb
                     ? cb.IsIndexBuffer ? "Index Buffer"
                     : cb.IsVertexBuffer ? "Vertex Buffer"
                     : string.Empty
                 : string.Empty;
        }

        private static string GetName(Parser.Asset.Base asset, Parser.DriverCall.Base dc)
            => dc.GetType().GetProperties().OfType<Resource>().FirstOrDefault(p => dc.Get<Resource>(p).Asset == asset)?.Name;

        private static ISlotResource GetResource(Parser.Asset.Base asset, IResourceSlots resourceSlots)
            => resourceSlots.AllSlots.FirstOrDefault(s => s.Asset == asset);
    }
}
