﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser
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

        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue? value)
            where TValue : struct
        {
            var success = !dict.TryGetValue(key, out var result);
            value = success ? null : (TValue?)result;
            return success;
        }
    }
}