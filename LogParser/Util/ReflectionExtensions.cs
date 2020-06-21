using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static void Set(this PropertyInfo prop, object value) => prop.SetValue(null, value);

        public static object Get(this PropertyInfo prop) => prop.GetValue(null);

        public static T Get<T>(this PropertyInfo prop) => (T)prop.GetValue(null);

        public static void Set(this object target, PropertyInfo prop, object value) => prop.SetValue(target, value);

        public static object Get(this object target, PropertyInfo prop) => prop.GetValue(target);

        public static T Get<T>(this object target, PropertyInfo prop) => (T)prop.GetValue(target);

        public static object Call(this object target, MethodInfo method, params object[] args) => method.Invoke(target, args);

        public static T Call<T>(this object target, MethodInfo method, params object[] args) => (T)method.Invoke(target, args);

        public static object Call(this MethodInfo method, params object[] args) => method.Invoke(null, args);

        public static T Call<T>(this MethodInfo method, params object[] args) => (T)method.Invoke(null, args);

        public static void SetFromString(this object target, string name, string value)
        {
            var prop = target.GetType().GetProperty(name);
            var converter = TypeDescriptor.GetProperties(target.GetType()).Find(name, false);
            target.Set(prop, converter.Converter.ConvertFromString(value));
        }

        public static void Add(this object target, PropertyInfo list, object value)
        {
            var add = list.PropertyType.GetMethod(nameof(ICollection<object>.Add));
            target.Get(list).Call(add, value);
        }

        public static bool IsGeneric(this PropertyInfo prop, Type genericType)
            => prop.PropertyType.IsGenericType && genericType == prop.PropertyType.GetGenericTypeDefinition();
    }
}
