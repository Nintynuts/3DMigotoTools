namespace Migoto.Config;

public class TextureOverride : Override<uint>
{
    public override string? HashFromString
    {
        set => Hash = uint.Parse(value ?? "0", NumberStyles.HexNumber);
    }
}
