using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Migoto.Log.Converter
{
    using Parser;
    using Parser.ApiCalls;
    using Parser.Assets;
    using Parser.Slots;

    internal static class AssetWriter
    {
        public static void Write(Asset asset, StreamWriter output)
        {
            var columns = new IColumns<ApiCall>[]
            {
                new Column<ApiCall, object>("Frame", dc => dc.Owner.Owner.Index),
                new Column<ApiCall, object>("From", dc => dc.Owner.Index),
                new Column<ApiCall, object>("To", dc => GetLastUser(asset, dc)?.Index),
                new Column<ApiCall, object>("Method", dc => dc.Name),
                new Column<ApiCall, object>("Slot", dc => GetResourceIdenfifier(asset, dc)),
                new Column<ApiCall, object>("Shader(s)", dc => $"\"{asset.GetShadersUntilOverriden(dc).Select(s => s?.Hex).Delimit('\n')}\""),
            };

            output.WriteLine($"Type:,{GetAssetSubType(asset)},{asset.GetType().Name}");
            output.WriteLine($"Slot,Count");
            asset.Slots.ForEach(s => output.WriteLine($"{s.index},{s.slots.Count}"));
            output.WriteLine(columns.Headers());
            asset.LifeCycle.ForEach(dc => output.WriteLine(columns.Values(dc)));
            output.Close();
        }

        private static DrawCall GetLastUser(Asset asset, ApiCall dc)
            => (dc is IMultiSlot multiSlot ? GetResource(asset, multiSlot).LastUser?.Owner : null) ?? dc.LastUser ?? dc.Owner;

        private static object GetResourceIdenfifier(Asset asset, ApiCall dc)
            => dc is IMultiSlot multiSlot ? (object)GetResource(asset, multiSlot).Index : GetResourceName(asset, dc);

        private static IResourceSlot GetResource(Asset asset, IMultiSlot multiSlot)
            => multiSlot.AllSlots.FirstOrDefault(s => s?.Asset == asset);

        private static string GetResourceName(Asset asset, ApiCall dc)
            => dc.GetType().GetProperties().OfType<Resource>().FirstOrDefault(p => dc.Get<Resource>(p).Asset == asset)?.Name;

        private static IEnumerable<Shader> GetShadersUntilOverriden(this Asset asset, ApiCall MethodBase)
        {
            var drawCalls = GetDrawCalls(MethodBase.Owner, GetLastUser(asset, MethodBase)).ToList();
            return drawCalls.Select(dc => GetShader(MethodBase, dc)).Distinct().ToList();
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

        private static Shader GetShader(ApiCall MethodBase, DrawCall drawCall)
            => GetShaderContext(MethodBase, drawCall)?.SetShader.Shader;

        private static ShaderContext GetShaderContext(ApiCall MethodBase, DrawCall drawCall)
        {
            return MethodBase is IShaderCall shaderCall ? drawCall.Shader(shaderCall.ShaderType)
                 : MethodBase is IInputAssembler ? drawCall.Shader(ShaderType.Vertex)
                 : MethodBase is IOutputMerger ? drawCall.Shader(ShaderType.Pixel)
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
