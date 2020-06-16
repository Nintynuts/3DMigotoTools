using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Migoto.Log.Parser
{
    public static class ReflectionExtensions
    {
        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue? value)
            where TValue : struct
        {
            var success = !dict.TryGetValue(key, out var result);
            value = success ? null : (TValue?)result;
            return success;
        }

        public static T Construct<T>(this Type type, params object[] args)
        {
            return (T)Activator.CreateInstance(type, args);
        }

        public static void SetFromString(this object target, string name, string value)
        {
            var prop = target.GetType().GetProperty(name);
            var converter = TypeDescriptor.GetProperties(target.GetType()).Find(name, false);
            prop.SetValue(target, converter.Converter.ConvertFromString(value));
        }

        public static void AddWithRefection(this PropertyInfo listProperty, object target, object value)
        {
            listProperty.PropertyType.GetMethod("Add").Invoke(listProperty.GetValue(target), new[] { value });
        }
    }
}
