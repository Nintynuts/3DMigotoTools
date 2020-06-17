using System.Collections.Generic;

namespace Migoto.Log.Parser
{
    public class Frame
    {
        public Frame(uint index)
        {
            Index = index;
        }

        public object Index { get; }
        public List<DrawCall> DrawCalls { get; } = new List<DrawCall>();
    }
}
