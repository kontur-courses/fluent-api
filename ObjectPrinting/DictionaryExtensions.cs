using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> destination)
        {
            foreach (var element in destination)
                source.Add(element.Key, element.Value);
        }

        public static void AddRule<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (!source.TryAdd(key, value))
                throw new InvalidOperationException("there is some rule for this type or member");
        }
    }
}
