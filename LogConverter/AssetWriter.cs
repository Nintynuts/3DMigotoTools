using System.Collections.Generic;
using System.IO;
using System.Linq;

using Migoto.Log.Parser;
using Migoto.Log.Parser.Asset;
using Migoto.Log.Parser.DriverCall;
using Migoto.Log.Parser.Slot;

using Asset = Migoto.Log.Parser.Asset.Base;
using DriverCall = Migoto.Log.Parser.DriverCall.Base;

namespace Migoto.Log.Converter
{
    using Column = Column<DriverCall>;
    using IColumns = IColumns<DriverCall>;
    internal static class AssetWriter
    {
        public static void Write(Asset asset, StreamWriter output)
        {
            var columns = new IColumns[]
            {
                new Column("FirstDrawCall", dc => dc.Owner.Index),
                new Column("LastDrawCall", dc => GetLastUser(asset, dc)?.Index),
                new Column("DriverCall", dc => dc.Name),
                new Column("Slot", dc => dc is IResourceSlots resourceSlots ? (object)GetResource(asset, resourceSlots).Index : GetName(asset, dc)),
                new Column("Shader(s)", dc => $"\"{asset.GetShadersBetween(dc).Select(s => s.Hex).Delimit('\n')}\""),
            };

            output.WriteLine($"Type:,{GetAssetSubType(asset)},{asset.GetType().Name}");
            output.WriteLine(columns.Headers());
            asset.LifeCycle.ForEach(dc => output.WriteLine(columns.Values(dc)));
            output.Close();
        }

        private static IEnumerable<Shader> GetShadersBetween(this Asset asset, DriverCall driverCall)
        {
            var drawCalls = GetDrawCalls(driverCall.Owner, GetLastUser(asset, driverCall)).ToList();
            return drawCalls.Select(dc => GetShader(driverCall, dc)).Distinct().ToList();
        }

        private static DrawCall GetLastUser(Asset asset, DriverCall dc)
        {
            return (dc is IResourceSlots resourceSlots ? GetResource(asset, resourceSlots).LastUser?.Owner : null) ?? dc.LastUser ?? dc.Owner;
        }

        private static IEnumerable<DrawCall> GetDrawCalls(DrawCall firstUser, DrawCall lastUser)
        {
            var current = lastUser;
            yield return current;

            while (current != firstUser && current != null)
            {
                yield return current;
                current = current.Previous;
            }
        }

        private static Shader GetShader(DriverCall drivercall, DrawCall drawCall) => GetShaderContext(drivercall, drawCall)?.SetShader.Shader;

        private static ShaderContext GetShaderContext(DriverCall drivercall, DrawCall drawCall)
        {
            return drivercall is IShaderCall shaderCall ? drawCall.Shader(shaderCall.ShaderType)
                 : drivercall is IInputAssembler ? drawCall.Shader(ShaderType.Vertex)
                 : drivercall is IOutputMerger ? drawCall.Shader(ShaderType.Pixel)
                 : null;
        }

        private static string GetAssetSubType(Asset asset)
        {
            return asset is Texture texture
                     ? texture.IsRenderTarget ? "Render Target"
                     : texture.IsDepthStencil ? "Depth Stencil"
                     : string.Empty
                 : asset is Buffer cb
                     ? cb.IsIndexBuffer ? "Index"
                     : cb.IsVertexBuffer ? "Vertex"
                     : "Constant"
                 : string.Empty;
        }

        private static string GetName(Asset asset, DriverCall dc)
            => dc.GetType().GetProperties().OfType<Resource>().FirstOrDefault(p => dc.Get<Resource>(p).Asset == asset)?.Name;

        private static ISlotResource GetResource(Asset asset, IResourceSlots resourceSlots)
            => resourceSlots.AllSlots.FirstOrDefault(s => s.Asset == asset);
    }
}
