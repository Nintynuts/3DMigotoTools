using System;
using System.Globalization;

namespace Migoto.Log.Parser
{
    class IntFormatAttribute : Attribute
    {
        public IntFormatAttribute(NumberStyles style)
        {
            Style = style;
        }

        public NumberStyles Style { get; }
    }
}
