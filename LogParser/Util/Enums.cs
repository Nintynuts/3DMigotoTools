using System;
using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser
{
    public static class Enums
    {
        public static T Parse<T>(string name) where T : Enum => (T)Enum.Parse(typeof(T), name, true);

        public static IEnumerable<T> Values<T>() where T : Enum => Enum.GetValues(typeof(T)).OfType<T>();
    }
}
