﻿namespace Migoto.Log.Parser.DriverCall
{
    public class ShaderContext : IDeferred<ShaderContext>
    {
        public Deferred<ShaderContext> Deferred { get; }

        public ShaderContext(ShaderType shaderType, DrawCall owner, DrawCall previous)
        {
            ShaderType = shaderType;
            Owner = owner;
            Deferred = new Deferred<ShaderContext>(previous?.Shader(shaderType));
        }

        public ShaderType ShaderType { get; }
        public DrawCall Owner { get; }
        public SetShader SetShader { get => Deferred.Get<SetShader>(); set => Deferred.Set(value); }

        public SetSamplers SetSamplers { get => Deferred.Get<SetSamplers>(); set => Deferred.Set(value); }

        public SetShaderResources SetShaderResources { get => Deferred.Get<SetShaderResources>(); set => Deferred.Set(value); }

        public SetConstantBuffers SetConstantBuffers { get => Deferred.Get<SetConstantBuffers>(); set => Deferred.Set(value); }
    }
}
