using System.Collections.Generic;

namespace Migoto.Log.Parser
{
    public class Frame
    {
        public Frame(uint index)
        {
            Index = index;
            DrawCalls = new OwnedCollection<Frame, DrawCall>(this);
        }

        public object Index { get; }
        public ICollection<DrawCall> DrawCalls { get; }
    }
}
