using System.Collections.Generic;

namespace ObjectPrinting.Extensions;

public static class HashsetExtensions
{
    public static void AddRange<T>(this HashSet<T> current, HashSet<T> toAdd)
    {
        foreach (var item in toAdd)
            current.Add(item);
    }
}