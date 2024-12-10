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
}