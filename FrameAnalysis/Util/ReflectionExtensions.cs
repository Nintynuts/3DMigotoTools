using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Migoto.Log.Parser
{
    public static class ReflectionExtensions
    {
        public static object Construct(this Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        public static T Construct<T>(this Type type, params object[] args)
        {
            return (T)Activator.CreateInstance(type, args);
        }

        #region Static
        public static void Set(this PropertyInfo prop, object value) => prop.SetValue(null, value);

        public static object Get(this PropertyInfo prop) => prop.GetValue(null);

        public static T Get<T>(this PropertyInfo prop) => (T)prop.GetValue(null);

        public static object Call(this MethodInfo method, params object[] args) => method.Invoke(null, args);

        public static T Call<T>(this MethodInfo method, params object[] args) => (T)method.Invoke(null, args);
        #endregion

        #region Instance
        public static void SetTo(this PropertyInfo prop, object target, object value) => prop.SetValue(target, value);

        public static object GetFrom(this PropertyInfo prop, object target) => prop.GetValue(target);

        public static T GetFrom<T>(this PropertyInfo prop, object target) => (T)prop.GetValue(target);

        public static object CallOn(this MethodInfo method, object target, params object[] args) => method.Invoke(target, args);

        public static T CallOn<T>(this MethodInfo method, object target, params object[] args) => (T)method.Invoke(target, args);
        #endregion

        public static void SetFromString(this object target, string name, string value)
        {
            var prop = target.GetType().GetProperty(name);
            var converter = TypeDescriptor.GetProperties(target.GetType()).Find(name, false);
            prop.SetTo(target, converter.Converter.ConvertFromString(value));
        }

        public static void Add(this object target, PropertyInfo list, object value)
        {
            var add = list.PropertyType.GetMethod(nameof(ICollection<object>.Add));
            add.CallOn(list.GetFrom(target), value);
        }

        public static bool IsGeneric(this PropertyInfo prop, Type genericType)
            => prop.PropertyType.IsGenericType && genericType == prop.PropertyType.GetGenericTypeDefinition();

        public static Type FirstType(this PropertyInfo prop)
            => prop.PropertyType.GetGenericArguments()[0];

        public static IEnumerable<PropertyInfo> OfType<T>(this PropertyInfo[] properties)
            => properties.Where(p => typeof(T).IsAssignableFrom(p.PropertyType));

        public static bool Is<T>(this Type type)
            => typeof(T).IsAssignableFrom(type);
    }
}
