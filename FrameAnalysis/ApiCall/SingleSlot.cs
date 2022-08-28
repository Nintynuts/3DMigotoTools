
namespace Migoto.Log.Parser.ApiCalls;

using Assets;
using Slots;

public interface ISingleSlot
{
    IResource? Slot { get; }
}

public interface IAssetSlot : ISingleSlot
{
    void UpdateAsset(Asset asset);
}

public abstract class SingleSlot<T> : ApiCall, IAssetSlot, IResource
    where T : Asset
{
    protected SingleSlot(uint order) : base(order) { }

    public T? Asset { get; protected set; }

    IApiCall IResource.Owner => this;
    Asset? IResource.Asset => Asset;
    ulong IResource.Pointer => Pointer;
    protected ulong Pointer { get; set; }

    IResource ISingleSlot.Slot => this;

    public void UpdateAsset(Asset asset)
    {
        if (asset is not T assetT)
            throw new InvalidDataException("Trying to change type of asset");
        Asset?.Unregister(this);
        Asset = assetT;
        Asset?.Register(this);
    }
}
