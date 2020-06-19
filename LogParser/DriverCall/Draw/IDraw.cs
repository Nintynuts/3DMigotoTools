namespace Migoto.Log.Parser.DriverCall.Draw
{
    public interface IDraw
    {
        uint? StartVertex { get; }
        uint? EndVertex { get; }
        uint? StartIndex { get; }
        uint? EndIndex { get; }
        uint? StartInstance { get; }
        uint? EndInstance { get; }
    }
}
