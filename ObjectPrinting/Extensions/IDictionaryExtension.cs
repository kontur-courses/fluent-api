using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting
{
    public static class IDictionaryExtension
    {
        public static IEnumerable<T> Cast<T>(this IDictionary dictionary)
        {
            foreach (T item in dictionary)
                yield return item;
        }

        public static TT GetValueOrDefault<T, TT>(this IDictionary<T, TT> dictionary, T key)
        {
            if (dictionary.Cast<KeyValuePair<T, TT>>()
                .ToDictionary(entry => entry.Key, entry => entry.Value)
                .TryGetValue(key, out TT value))
            {
                return value;
            }
            return default;
        }
    }
}