using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Migoto.Log.Parser
{
    public class Deferred<T>
    {
        private Dictionary<string, object> Overrides { get; } = new Dictionary<string, object>();

        private T Fallback { get; }

        public Deferred(T fallback)
        {
            Fallback = fallback;
        }

        public TProperty Get<TProperty>(bool useFallback = true, [CallerMemberName] string name = null)
            where TProperty : class
        {
            if (Overrides.ContainsKey(name))
                return (TProperty)Overrides[name];
            else if (useFallback && Fallback != null)
                return (TProperty)typeof(T).GetProperty(name).GetValue(Fallback);
            else
                return null;
        }

        public void Set<TProperty>(TProperty value, bool warnIfNotNull = true, [CallerMemberName] string name = null)
            where TProperty : class
        {
            if (warnIfNotNull && Overrides.ContainsKey(name))
                Console.WriteLine($"{value.GetType().Name} already registered");

            Overrides[name] = value;
        }
    }
}
