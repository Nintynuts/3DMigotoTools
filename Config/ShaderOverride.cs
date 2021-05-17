using System.Globalization;

namespace Migoto.Config
{
    public class ShaderOverride : Override<ulong>
    {
        public override string? HashFromString
        {
            set => Hash = ulong.Parse(value ?? "0", NumberStyles.HexNumber);
        }
    }
}
