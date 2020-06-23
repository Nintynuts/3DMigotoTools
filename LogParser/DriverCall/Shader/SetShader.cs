namespace Migoto.Log.Parser.ApiCalls
{
    using Assets;

    public class SetShader : ApiCall
    {
        public SetShader(uint order) : base(order) { }

        public ulong pShader { get; set; }
        public ulong ppClassInstances { get; set; }
        public uint NumClassInstances { get; set; }

        public Shader Shader { get; set; }
    }
}
