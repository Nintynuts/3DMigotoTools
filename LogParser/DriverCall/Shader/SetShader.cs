using Migoto.Log.Parser.Asset;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetShader : Base
    {
        public SetShader(uint order, DrawCall owner) : base(order, owner)
        {
        }

        public ulong pShader { get; set; }

        public ulong ppClassInstances { get; set; }

        public uint NumClassInstances { get; set; }

        public Shader Shader { get; set; }
    }
}
