using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

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
            var columns = new IColumns<IApiCall>[]
            {
                new Column<IApiCall, object?>("Frame", dc => dc.Owner?.Owner?.Index),
                new Column<IApiCall, object?>("From", dc => dc.Owner?.Index),
                new Column<IApiCall, object?>("To", dc => GetLastUser(asset, dc)?.Index),
                new Column<IApiCall, object>("Method", dc => dc.Name),
                new Column<IApiCall, object>("Slot", dc => GetResourceIdenfifier(asset, dc)),
                new Column<IApiCall, object>("Shader(s)", dc => $"\"{asset.GetShadersUntilOverriden(dc).ExceptNull().Select(s => s.Hex).Delimit('\n')}\""),
            };

            output.WriteLine($"Type:,{GetAssetSubType(asset)},{asset.GetType().Name}");
            if (asset.Override != null)
                output.WriteLine($"Override:,{asset.Override.Name}");
            output.WriteLine();
            output.WriteLine($"Slot,Count,Variable");
            asset.Slots.ForEach(s => output.WriteLine($"{s.index},{s.slots.Count},{asset.GetNameForSlot(s.index)}"));
            output.WriteLine();
            output.WriteLine(columns.Headers());
            asset.LifeCycle.ForEach(dc => output.WriteLine(columns.Values(dc)));
        }

        private static DrawCall? GetLastUser(Asset asset, IApiCall dc)
            => (TryGetResource(asset, dc, out var resource) ? resource.LastUser?.Owner : null) ?? dc.LastUser ?? dc.Owner;

        private static object GetResourceIdenfifier(Asset asset, IApiCall dc)
            => TryGetResource(asset, dc, out var resource) ? resource.Index : GetResourceName(asset, dc);

        private static bool TryGetResource(Asset asset, IApiCall dc, [NotNullWhen(true)] out IResourceSlot resource)
        {
            resource = null!;
            return dc is IMultiSlot multiSlot && GetResource(asset, multiSlot) is { } result && (resource = result) == result;
        }

        private static IResourceSlot? GetResource(Asset asset, IMultiSlot multiSlot)
            => multiSlot.Slots.FirstOrDefault(s => s?.Asset == asset);

        private static string GetResourceName(Asset asset, IApiCall dc)
            => dc.GetType().GetProperties().OfType<Resource>().FirstOrDefault(p => p.GetFrom<Resource>(dc).Asset == asset)?.Name ?? string.Empty;

        private static IEnumerable<Shader> GetShadersUntilOverriden(this Asset asset, IApiCall MethodBase)
        {
            if (MethodBase.Owner == null)
                return Enumerable.Empty<Shader>();

            var drawCalls = GetDrawCalls(MethodBase.Owner, GetLastUser(asset, MethodBase)).ToList();
            return drawCalls.Select(dc => GetShader(MethodBase, dc)).ExceptNull().Distinct().ToList();
        }

        private static IEnumerable<DrawCall> GetDrawCalls(DrawCall firstUser, DrawCall? lastUser)
        {
            if (lastUser == null)
                yield break;

            var current = lastUser;
            yield return current;

            while (current != firstUser && current != null)
            {
                yield return current;
                current = current.Fallback;
            }
        }

        private static Shader? GetShader(IApiCall MethodBase, DrawCall drawCall)
            => GetShaderContext(MethodBase, drawCall)?.SetShader?.Shader;

        private static ShaderContext? GetShaderContext(IApiCall MethodBase, DrawCall drawCall)
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
