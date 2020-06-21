using System.Linq;

using Migoto.Log.Parser.DriverCall;

namespace Migoto.Log.Parser.Asset
{
    public class ConstantBuffer : Base
    {
        public bool IsIndexBuffer => Uses.Any(s => s.Owner is IASetIndexBuffer);
        public bool IsVertexBuffer => Uses.Any(s => s.Owner is IASetVertexBuffers);
    }
}
