using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static ICollection<T> Consolidate<T>(this IEnumerable<T> items)
            => items.Where(s => s != null).Distinct().ToList();

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? items) => items ?? Enumerable.Empty<T>();

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable? items) => (items?.OfType<T>()) ?? Enumerable.Empty<T>();

        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue? value)
            where TKey : notnull
            where TValue : struct
        {
            var success = !dict.TryGetValue(key, out var result);
            value = success ? null : (TValue?)result;
            return success;
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> makeNew)
            where TKey : notnull
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, makeNew());
            return dict[key];
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
            where TKey : notnull
            where TValue : new()
        {
            return dict.GetOrAdd(key, () => new TValue());
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
            where TKey : notnull
            where TValue : struct
        {
            return dict.GetOrAdd(key, () => defaultValue);
        }

        public static IEnumerable<T> ExceptNull<T>(this IEnumerable<T?> items) where T : notnull
            => items.OfType<T>();

        public static void RemoveAll<T>(this ICollection<T> items, Func<T, bool> filter)
            => items.Where(filter).ToList().ForEach(i => items.Remove(i));

        public static string Delimit(this IEnumerable<string> items, char delimiter)
            => items.Delimit($"{delimiter}");

        public static string Delimit(this IEnumerable<string> items, string delimiter)
            => items.Any() ? string.Join(delimiter, items) : string.Empty;

        public static IEnumerable<T> Indices<T>(this IReadOnlyList<T> items, IEnumerable<int> indices)
            => indices.Select(i => items[i]);
    }
}
