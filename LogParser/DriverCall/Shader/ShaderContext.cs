namespace Migoto.Log.Parser.DriverCall
{
    public class ShaderContext : Base
    {
        private readonly Deferred<ShaderContext> deferred;

        public ShaderContext(ShaderType shaderType, DrawCall owner, ShaderContext previous) : base(owner)
        {
            ShaderType = shaderType;
            deferred = new Deferred<ShaderContext>(previous);
        }

        public ShaderType ShaderType { get; }

        public SetShader SetShader { get => deferred.Get<SetShader>(); set => deferred.Set(value); }

        public SetSamplers SetSamplers { get => deferred.Get<SetSamplers>(); set => deferred.Set(value); }

        public SetShaderResources SetShaderResources { get => deferred.Get<SetShaderResources>(); set => deferred.Set(value); }

        public SetConstantBuffers SetConstantBuffers { get => deferred.Get<SetConstantBuffers>(); set => deferred.Set(value); }
    }
}
