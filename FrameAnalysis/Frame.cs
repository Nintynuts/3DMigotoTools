namespace Migoto.Log.Parser
{
    public class Frame
    {
        public Frame(uint index)
        {
            Index = index;
            DrawCalls = new OwnedCollection<Frame, DrawCall>(this);
        }

        public uint Index { get; }

        public IIndexedCollection<DrawCall> DrawCalls { get; }
    }
}
