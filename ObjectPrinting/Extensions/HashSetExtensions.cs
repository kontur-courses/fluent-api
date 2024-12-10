using System.Collections.Generic;

namespace ObjectPrinting.Extensions;

public static class HashSetExtensions
{
    public static HashSet<T> AddRange<T>(this HashSet<T> current, HashSet<T> toAdd)
    {
        foreach (var item in toAdd)
            current.Add(item);

        return current;
    }
}