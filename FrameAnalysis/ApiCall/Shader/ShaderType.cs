using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser.ApiCalls
{
    public enum ShaderType
    {
        Vertex, Hull, Domain, Geometry, Pixel, Compute
    }

    public static class ShaderTypes
    {
        public static Dictionary<char, ShaderType> FromLetter { get; } = Enums.Values<ShaderType>().ToDictionary(s => s.Letter(), s => s);

        public static char Letter(this ShaderType shaderType) => shaderType.ToString()[0];
    }
}
