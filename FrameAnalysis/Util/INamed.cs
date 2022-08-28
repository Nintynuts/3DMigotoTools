namespace Migoto.Log.Parser;

public interface INamed
{
    public string Name => GetType().Name;
}
