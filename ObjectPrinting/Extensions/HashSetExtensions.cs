using System.Collections.Generic;

namespace ObjectPrinting.Extensions
{
    public static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> source, HashSet<T> destination)
        {
            foreach (var element in destination)
                source.Add(element);
        }
    }
}
