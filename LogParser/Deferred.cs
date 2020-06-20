using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Migoto.Log.Parser
{
    public interface IDeferred<T> where T : IDeferred<T>
    {
        Deferred<T> Deferred { get; }
    }

    public class Deferred<T> where T : IDeferred<T>
    {
        protected Dictionary<string, object> Overrides { get; } = new Dictionary<string, object>();

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
            {
                // This way avoids stack overflow
                var fallback = Fallback;
                object result;
                while (!fallback.Deferred.Overrides.TryGetValue(name, out result) && fallback.Deferred.Fallback != null)
                    fallback = fallback.Deferred.Fallback;

                return (TProperty)result;
            }
            else
                return null;
        }

        public void Set<TProperty>(TProperty value, bool warnIfExists = true, [CallerMemberName] string name = null)
            where TProperty : class
        {
            if (Overrides.ContainsKey(name))
            {
                if (Overrides[name] is IMergable<TProperty> mergable)
                {
                    mergable.Merge(value);
                    return;
                }
                Overrides[name] = value;
                if (warnIfExists)
                    throw new ArgumentException($"{value.GetType().Name} already registered");
            }
            else
            {
                Overrides[name] = value;
            }
        }
    }
}
