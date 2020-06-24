

namespace Migoto.Log.Parser.Slots
{
    using ApiCalls;
    using Assets;

    public interface IResource
    {
        Asset Asset { get; }

        ApiCall Owner { get; }

        ulong Pointer { get; }

        void UpdateAsset(Asset asset);
    }

    public interface IResourceSlot : IResource, IOverriden<ApiCall>
    {
        int Index { get; }
    }
}