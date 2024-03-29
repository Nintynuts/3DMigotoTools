﻿namespace Migoto.Log.Parser;

using ApiCalls;
using ApiCalls.Draw;

using OMGetRTsAndUAVs = ApiCalls.OMGetRenderTargetsAndUnorderedAccessViews;
using OMSetRTsAndUAVs = ApiCalls.OMSetRenderTargetsAndUnorderedAccessViews;

public class DrawCall : IOwned<Frame>, IDeferred<DrawCall, DrawCall>
{
    public Frame? Owner { get; set; }
    public Deferred<DrawCall, DrawCall> Deferred { get; }
    public DrawCall? Fallback { get; }

    public DrawCall(uint index, DrawCall? previous)
    {
        Index = index;
        Fallback = previous;
        Deferred = new Deferred<DrawCall, DrawCall>(this, previous);
        Shaders = Enums.Values<ShaderType>().ToDictionary(s => s, s => new ShaderContext(this, previous?.Shaders[s]));

        Mappings = new OwnedCollection<DrawCall, Map>(this);
        Unmappings = new OwnedCollection<DrawCall, Unmap>(this);
        ResourceCopied = new OwnedCollection<DrawCall, CopyResource>(this);
        SubresourceRegionCopied = new OwnedCollection<DrawCall, CopySubresourceRegion>(this);
        SubresourceUpdated = new OwnedCollection<DrawCall, UpdateSubresource>(this);
        DepthStencilCleared = new OwnedCollection<DrawCall, ClearDepthStencilView>(this);
        RenderTargetCleared = new OwnedCollection<DrawCall, ClearRenderTargetView>(this);
        UnorderedAccessViewCleared = new OwnedCollection<DrawCall, ClearUnorderedAccessViewUint>(this);
    }

    public uint Index { get; }

    public string? Logic { get; set; }

    public IDraw? Draw { get; set; }

    public ICollection<Map> Mappings { get; }
    public ICollection<Unmap> Unmappings { get; }

    public ICollection<CopyResource> ResourceCopied { get; }
    public ICollection<CopySubresourceRegion> SubresourceRegionCopied { get; }
    public ICollection<UpdateSubresource> SubresourceUpdated { get; }
    public ICollection<ClearDepthStencilView> DepthStencilCleared { get; }
    public ICollection<ClearRenderTargetView> RenderTargetCleared { get; }
    public ICollection<ClearUnorderedAccessViewUint> UnorderedAccessViewCleared { get; }

    public RSSetState? RasterizerState { get => Deferred.Get<RSSetState>(); set => Deferred.Set(value); }
    public RSSetScissorRects? RasterizerScissorRects { get => Deferred.Get<RSSetScissorRects>(); set => Deferred.Set(value); }
    public RSSetViewports? Viewports { get => Deferred.Get<RSSetViewports>(); set => Deferred.Set(value); }

    public OMSetRenderTargets? SetRenderTargets { get => Deferred.Get<OMSetRenderTargets>(); set => Deferred.Set(value); }
    public OMSetBlendState? BlendState { get => Deferred.Get<OMSetBlendState>(); set => Deferred.Set(value); }
    public OMSetDepthStencilState? DepthStencilState { get => Deferred.Get<OMSetDepthStencilState>(); set => Deferred.Set(value); }
    public OMGetRTsAndUAVs? GetRTsAndUAVs { get => Deferred.Get<OMGetRTsAndUAVs>(false); set => Deferred.Set(value); }
    public OMSetRTsAndUAVs? SetRTsAndUAVs { get => Deferred.Get<OMSetRTsAndUAVs>(false); set => Deferred.Set(value); }

    public IASetPrimitiveTopology? PrimitiveTopology { get => Deferred.Get<IASetPrimitiveTopology>(); set => Deferred.Set(value); }

    public IASetInputLayout? InputLayout { get => Deferred.Get<IASetInputLayout>(); set => Deferred.Set(value); }

    public IASetVertexBuffers? SetVertexBuffers { get => Deferred.Get<IASetVertexBuffers>(); set => Deferred.Set(value); }

    public IASetIndexBuffer? SetIndexBuffer { get => Deferred.Get<IASetIndexBuffer>(); set => Deferred.Set(value); }

    public IReadOnlyDictionary<ShaderType, ShaderContext> Shaders { get; }

    public void SetOwner(Frame? newOwner) => Owner = newOwner;

    public IEnumerable<string> MergeWarnings => Deferred.OfType<IMergable>().SelectMany(m => m.MergeWarnings);

    public IEnumerable<string> Collisions => Deferred.Collisions.Concat(Shaders.Values.Select(s => s.Deferred.Collisions).SelectMany(c => c));
}
