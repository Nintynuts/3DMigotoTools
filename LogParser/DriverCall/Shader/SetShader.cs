using Migoto.Log.Parser.Asset;

namespace Migoto.Log.Parser.DriverCall
{
    public class SetShader : Base
    {
        public SetShader(DrawCall owner) : base(owner)
        {
        }

        public uint pShader { get; set; }

        public uint ppClassInstances { get; set; }

        public uint NumClassInstances { get; set; }

        public Shader Shader { get; set; }
    }
}
