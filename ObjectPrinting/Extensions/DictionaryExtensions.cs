using System.Collections.Generic;

namespace ObjectPrinting
{
    internal static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>
            (this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
    }
}