namespace Migoto.Log.Parser.Assets
{
    using Config;

    public interface IConfigOverride<THash> where THash : struct
    {
        THash Hash { get; }

        Override<THash>? Override { get; set; }
    }
}
