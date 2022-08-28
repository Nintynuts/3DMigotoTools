namespace Migoto.Log.Parser.ApiCalls;

public class ShaderContext : IDeferred<ShaderContext, DrawCall>
{
    public Deferred<ShaderContext, DrawCall> Deferred { get; }

    public ShaderContext(DrawCall owner, ShaderContext? fallback)
    {
        Owner = owner;
        Fallback = fallback;
        Deferred = new Deferred<ShaderContext, DrawCall>(owner, fallback);
    }

    public DrawCall Owner { get; }

    public ShaderContext? Fallback { get; }

    public SetShader? SetShader { get => Deferred.Get<SetShader>(); set => Deferred.Set(value); }

    public SetSamplers? SetSamplers { get => Deferred.Get<SetSamplers>(); set => Deferred.Set(value); }

    public SetShaderResources? SetShaderResources { get => Deferred.Get<SetShaderResources>(); set => Deferred.Set(value); }

    public SetConstantBuffers? SetConstantBuffers { get => Deferred.Get<SetConstantBuffers>(); set => Deferred.Set(value); }
}
