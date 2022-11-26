using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting.Infrastructure;

public class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
{
    public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
    {
        return ReferenceEquals(x, y) || (x is not null && y is not null && x.SequenceEqual(y));
    }

    public int GetHashCode(IEnumerable<T> obj)
    {
        unchecked
        {
            return obj
                .Select(e => e?.GetHashCode() ?? 0)
                .Aggregate(17, (a, b) => 23 * a + b);
        }
    }
}