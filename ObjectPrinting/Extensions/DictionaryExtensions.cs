using System.Collections.Generic;

namespace ObjectPrinting.Extensions;

public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> current, Dictionary<TKey, TValue> toAdd)
        where TKey : notnull
    {
        foreach (var item in toAdd)
            current.Add(item.Key, item.Value);
    }

    public static void AddMethod<TKey, TValue>(this Dictionary<TKey, TValue> current, TKey key, TValue value)
        where TKey : notnull
    {
        if (!current.TryAdd(key, value))
            throw new InvalidOperationException($"Type {typeof(TKey).Name} is already registered");
    }
}