using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Migoto.Log.Parser.DriverCall
{
    public abstract class Base : IOwned<DrawCall>
    {
        public uint Order { get; }
        public DrawCall Owner { get; private set; }

        public void SetOwner(DrawCall newOwner) => Owner = newOwner;

        protected Base(uint order, DrawCall owner)
        {
            Order = order;
            Owner = owner;
        }

        public PropertyInfo SlotsProperty
            => GetType().GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>));

        public PropertyInfo AssetProperty
            => GetType().GetProperties().FirstOrDefault(p => p.CanWrite && typeof(Asset.Base).IsAssignableFrom(p.PropertyType));
    }
}
