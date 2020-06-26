

namespace Migoto.Log.Parser.Slots
{
    using ApiCalls;
    using Assets;

    public interface IResource
    {
        Asset Asset { get; }

        IApiCall Owner { get; }

        ulong Pointer { get; }

        void UpdateAsset(Asset asset);
    }

    public interface IResourceSlot : IResource, ISlot<IApiCall>, IOverriden<IApiCall>
    {
    }
}