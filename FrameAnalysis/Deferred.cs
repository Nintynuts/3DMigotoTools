using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Migoto.Log.Parser
{
    public interface IDeferred<TFallback, TOwner>
        where TFallback : class, IDeferred<TFallback, TOwner>
        where TOwner : class
    {
        Deferred<TFallback, TOwner> Deferred { get; }

        TFallback? Fallback { get; }
    }

    public class Deferred<TFallback, TOwner>
        where TFallback : class, IDeferred<TFallback, TOwner>
        where TOwner : class
    {
        private readonly TOwner owner;
        private readonly List<string> collisions = new List<string>();

        protected Dictionary<string, IOwned<TOwner>> Overrides { get; } = new Dictionary<string, IOwned<TOwner>>();

        protected Dictionary<string, IOwned<TOwner>> FallbackValues { get; } = new Dictionary<string, IOwned<TOwner>>();

        public IEnumerable<T> OfType<T>() => Overrides.Values.OfType<T>();

        public IEnumerable<object> OfType(Type type) => Overrides.Values.Where(v => v.GetType() == type);

        public IEnumerable<string> Collisions => collisions;

        private TFallback? Fallback { get; }

        public Deferred(TOwner owner, TFallback? fallback)
        {
            this.owner = owner;
            Fallback = fallback;
        }

        public TProperty? Get<TProperty>(bool useFallback = true, [CallerMemberName] string name = null)
            where TProperty : class
        {
            if (Overrides.TryGetValue(name, out var result))
                return (TProperty)result;

            if (!useFallback || Fallback == null)
                return null;

            if (!FallbackValues.TryGetValue(name, out result))
            {
                // This way avoids stack overflow
                var deferred = Fallback.Deferred;
                while (!deferred.Overrides.TryGetValue(name, out result)
                    && !deferred.FallbackValues.TryGetValue(name, out result)
                    && deferred.Fallback != null)
                {
                    deferred = deferred.Fallback.Deferred;
                }
                if (result != null)
                    FallbackValues[name] = result;
            }

            if (result != null)
                SetLastUser(result);
            return (TProperty?)result;
        }

        private void SetLastUser(IOwned<TOwner> result)
        {
            if (result is IOverriden<TOwner> previous)
                previous.SetLastUser(owner);
        }

        public void Set<TProperty>(TProperty? value, bool warnIfExists = true, [CallerMemberName] string name = null)
            where TProperty : IOwned<TOwner>
        {
            if (value == null)
            {
                Overrides.Remove(name);
                return;
            }
            if (Overrides.TryGetValue(name, out var existing))
            {
                if (existing is IMergable<TProperty> mergable)
                {
                    mergable.Merge(value);
                    return;
                }
                Overrides.Remove(name);
                if (warnIfExists)
                    collisions.Add($"{value.GetType().Name}: Already registered");
            }
            Overrides[name] = value;
            value.SetOwner(owner);
        }
    }
}
