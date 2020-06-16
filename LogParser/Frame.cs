using System.Collections.Generic;

namespace Migoto.Log.Parser
{
    public class Frame
    {
        public List<DrawCall> DrawCalls { get; } = new List<DrawCall>();
    }
}
