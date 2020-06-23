using System.Linq;

namespace Migoto.Log.Parser.Assets
{
    using ApiCalls;

    public class Buffer : Asset
    {
        public bool IsIndexBuffer => Uses.Any(s => s.Owner is IASetIndexBuffer);
        public bool IsVertexBuffer => Uses.Any(s => s.Owner is IASetVertexBuffers);
    }
}
