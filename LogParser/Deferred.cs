using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Migoto.Log.Parser
{
    public interface IDeferred<TDeferred, TOwner>
        where TDeferred : class, IDeferred<TDeferred, TOwner>
        where TOwner : class
    {
        Deferred<TDeferred, TOwner> Deferred { get; }
    }

    public class Deferred<TDeferred, TOwner>
        where TDeferred : class, IDeferred<TDeferred, TOwner>
        where TOwner : class
    {
        private readonly TOwner owner;
        private readonly List<string> collisions = new List<string>();

        protected Dictionary<string, IOwned<TOwner>> Overrides { get; } = new Dictionary<string, IOwned<TOwner>>();

        public IEnumerable<T> Values<T>() => Overrides.Values.OfType<T>();

        public IEnumerable<string> Collisions => collisions;

        private TDeferred Fallback { get; }

        public Deferred(TOwner owner, TDeferred fallback)
        {
            this.owner = owner;
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
                IOwned<TOwner> result;
                while (!fallback.Deferred.Overrides.TryGetValue(name, out result) && fallback.Deferred.Fallback != null)
                    fallback = fallback.Deferred.Fallback;

                return (TProperty)result;
            }
            else
                return null;
        }

        public void Set<TProperty>(TProperty value, bool warnIfExists = true, [CallerMemberName] string name = null)
            where TProperty : IOwned<TOwner>
        {
            if (Overrides.TryGetValue(name, out var existing))
            {
                if (existing is IMergable<TProperty> mergable)
                {
                    mergable.Merge(value);
                    return;
                }
                existing.SetOwner(null);
                Overrides[name] = value;
                value.SetOwner(owner);
                if (warnIfExists)
                    collisions.Add($"{value.GetType().Name}: Already registered");
            }
            else
            {
                Overrides[name] = value;
                value.SetOwner(owner);
            }
        }
    }
}
