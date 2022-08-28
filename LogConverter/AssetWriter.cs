namespace Migoto.Log.Converter;

using Parser;
using Parser.ApiCalls;
using Parser.Assets;
using Parser.Slots;

internal class AssetWriter
{
    private readonly Asset asset;
    private readonly IColumns<IApiCall>[] columns;

    public AssetWriter(Asset asset)
    {
        this.asset = asset;
        columns = new IColumns<IApiCall>[]
        {
            new Column<IApiCall, object?>("Frame", dc => dc.Owner?.Owner?.Index),
            new Column<IApiCall, object?>("From", dc => dc.Owner?.Index),
            new Column<IApiCall, object?>("To", dc => GetLastUser(dc)?.Index),
            new Column<IApiCall, object>("Method", dc => dc.Name),
            new Column<IApiCall, object>("Slot", dc => GetResourceIdenfifier(dc)),
            new Column<IApiCall, object>("Shader(s)", dc => $"\"{GetShadersUntilOverriden(dc).ExceptNull().Select(s => s.Hex).Delimit('\n')}\""),
        };
    }

    public void Write(Func<StreamWriter?> outputSrc)
    {
        using var output = outputSrc();

        if (output == null)
            throw new NullReferenceException(nameof(output));

        output.WriteLine($"Type:,{AssetSubType},{asset.GetType().Name}");
        if (asset.Override != null)
            output.WriteLine($"Override:,{asset.Override.Name}");
        output.WriteLine();
        output.WriteLine("Slot,Count,Variable");
        asset.Slots.ForEach(s => output.WriteLine($"{s.index},{s.slots.Count},{asset.GetNameForSlot(s.index)}"));
        output.WriteLine();
        output.WriteLine(columns.Headers());
        asset.LifeCycle.ForEach(dc => output.WriteLine(columns.Values(dc)));
    }

    private DrawCall? GetLastUser(IApiCall dc)
        => (TryGetResource(dc) is { } resource ? resource.LastUser?.Owner : null) ?? dc.LastUser ?? dc.Owner;

    private object GetResourceIdenfifier(IApiCall dc)
        => TryGetResource(dc) is { } resource ? resource.Index : GetResourceName(dc);

    private IResourceSlot? TryGetResource(IApiCall dc)
        => dc is IMultiSlot<IResourceSlot> multiSlot ? GetResource(multiSlot) : null;

    private IResourceSlot? GetResource(IMultiSlot<IResourceSlot> multiSlot)
        => multiSlot.Slots.FirstOrDefault(s => s?.Asset == asset);

    private string GetResourceName(IApiCall dc)
        => dc.GetType().GetProperties().OfType<Resource>().FirstOrDefault(p => p.GetFrom<Resource>(dc).Asset == asset)?.Name ?? string.Empty;

    private IEnumerable<Shader> GetShadersUntilOverriden(IApiCall methodBase)
    {
        if (methodBase.Owner == null)
            return Enumerable.Empty<Shader>();

        var drawCalls = GetDrawCalls(methodBase.Owner, GetLastUser(methodBase)).ToList();
        return drawCalls.Select(dc => GetShader(methodBase, dc)).ExceptNull().Distinct().ToList();
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

    private static Shader? GetShader(IApiCall methodBase, DrawCall drawCall)
        => GetShaderContext(methodBase, drawCall)?.SetShader?.Shader;

    private static ShaderContext? GetShaderContext(IApiCall methodBase, DrawCall drawCall) => methodBase switch
    {
        IShaderCall shaderCall => drawCall.Shaders[shaderCall.ShaderType],
        IInputAssembler => drawCall.Shaders[ShaderType.Vertex],
        IOutputMerger => drawCall.Shaders[ShaderType.Pixel],
        _ => null
    };

    private string AssetSubType
        => asset is Texture texture
             ? texture.IsRenderTarget ? "Render Target"
             : texture.IsDepthStencil ? "Depth Stencil"
             : string.Empty
         : asset is Buffer cb
             ? cb.IsIndexBuffer ? "Index"
             : cb.IsVertexBuffer ? "Vertex"
             : "Constant"
         : string.Empty;
}
