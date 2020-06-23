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
    internal static class AssetWriter
    {
        public static void Write(Asset asset, StreamWriter output)
        {
            var columns = new IColumns<DriverCall>[]
            {
                new Column<DriverCall, object>("Frame", dc => dc.Owner.Owner.Index),
                new Column<DriverCall, object>("From", dc => dc.Owner.Index),
                new Column<DriverCall, object>("To", dc => GetLastUser(asset, dc)?.Index),
                new Column<DriverCall, object>("Method", dc => dc.Name),
                new Column<DriverCall, object>("Slot", dc => GetResourceIdenfifier(asset, dc)),
                new Column<DriverCall, object>("Shader(s)", dc => $"\"{asset.GetShadersUntilOverriden(dc).Select(s => s?.Hex).Delimit('\n')}\""),
            };

            output.WriteLine($"Type:,{GetAssetSubType(asset)},{asset.GetType().Name}");
            output.WriteLine($"Slot,Count");
            asset.Slots.ForEach(s => output.WriteLine($"{s.index},{s.slots.Count}"));
            output.WriteLine(columns.Headers());
            asset.LifeCycle.ForEach(dc => output.WriteLine(columns.Values(dc)));
            output.Close();
        }

        private static DrawCall GetLastUser(Asset asset, DriverCall dc)
            => (dc is IResourceSlots resourceSlots ? GetResource(asset, resourceSlots).LastUser?.Owner : null) ?? dc.LastUser ?? dc.Owner;

        private static object GetResourceIdenfifier(Asset asset, DriverCall dc)
            => dc is IResourceSlots resourceSlots ? (object)GetResource(asset, resourceSlots).Index : GetResourceName(asset, dc);

        private static ISlotResource GetResource(Asset asset, IResourceSlots resourceSlots)
            => resourceSlots.AllSlots.FirstOrDefault(s => s?.Asset == asset);

        private static string GetResourceName(Asset asset, DriverCall dc)
            => dc.GetType().GetProperties().OfType<Resource>().FirstOrDefault(p => dc.Get<Resource>(p).Asset == asset)?.Name;

        private static IEnumerable<Shader> GetShadersUntilOverriden(this Asset asset, DriverCall driverCall)
        {
            var drawCalls = GetDrawCalls(driverCall.Owner, GetLastUser(asset, driverCall)).ToList();
            return drawCalls.Select(dc => GetShader(driverCall, dc)).Distinct().ToList();
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

        private static Shader GetShader(DriverCall drivercall, DrawCall drawCall)
            => GetShaderContext(drivercall, drawCall)?.SetShader.Shader;

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
    }
}
