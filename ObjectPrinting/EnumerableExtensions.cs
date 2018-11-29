using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<object> Take(this IEnumerable enumerable, int max)
        {
            var i = 0;
            foreach (var obj in enumerable)
            {
                if (i++ >= max)
                    yield break;
                yield return obj;
            }
        }
    }
}
