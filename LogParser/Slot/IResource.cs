
namespace Migoto.Log.Parser.Slot
{
    public interface IResource
    {
        Asset.Base Asset { get; }
        int Index { get; }
        DriverCall.Base Owner { get; }
        uint Pointer { get; }
    }
}