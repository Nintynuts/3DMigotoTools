
namespace Migoto.Log.Parser.Slot
{
    public interface IResource
    {
        Asset.Base Asset { get; }

        DriverCall.Base Owner { get; }

        ulong Pointer { get; }

        void UpdateAsset(Asset.Base asset);
    }

    public interface ISlotResource : IResource, IOverriden<DriverCall.Base>
    {
        int Index { get; }
    }
}