using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Migoto.Log.Parser.DriverCall
{
    public abstract class Base
    {
        public DrawCall Owner { get; }

        protected Base(DrawCall owner)
        {
            Owner = owner;
        }

        public PropertyInfo SlotsProperty
            => GetType().GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>));

        public PropertyInfo AssetProperty
            => GetType().GetProperties().FirstOrDefault(p => typeof(Asset.Base).IsAssignableFrom(p.PropertyType));
    }
}
