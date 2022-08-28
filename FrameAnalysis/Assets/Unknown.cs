namespace Migoto.Log.Parser.Assets;

class Unknown : Asset
{
    internal void ReplaceWith(Asset asset) => Uses.ToList().ForEach(s => s.UpdateAsset(asset));
}
