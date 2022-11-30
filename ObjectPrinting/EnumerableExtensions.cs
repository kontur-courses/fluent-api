using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public static class IEnumerableExtensions
    {
        public static object ElementAtOrDefault(this IEnumerable collection, int index)
        {
            foreach (var e in collection)
            {
                if (index == 0)
                    return e;
                index--;
            }

            return null!;
        }
    }
}
