using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Migoto.Log.Parser.DriverCall
{
    public abstract class Base : IOwned<DrawCall>, IOverriden<DrawCall>
    {
        public uint Order { get; }
        public DrawCall Owner { get; private set; }
        public DrawCall LastUser { get; private set; }

        public void SetOwner(DrawCall newOwner) => Owner = newOwner;
        public void SetLastUser(DrawCall lastUser) => LastUser = lastUser;

        public virtual string Name => GetType().Name;

        protected Base(uint order)
        {
            Order = order;
        }

        public PropertyInfo SlotsProperty
            => GetType().GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType
               && p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
               && typeof(Slot.Base).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]));

        public PropertyInfo AssetProperty
            => GetType().GetProperties().FirstOrDefault(p => p.CanWrite && typeof(Asset.Base).IsAssignableFrom(p.PropertyType));
    }
}
