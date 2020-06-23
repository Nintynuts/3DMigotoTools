using System.Linq;

namespace Migoto.Log.Parser.Asset
{
    class Unknown : Base
    {
        internal void ReplaceWith(Base asset) => Uses.ToList().ForEach(s => s.UpdateAsset(asset));
    }
}
