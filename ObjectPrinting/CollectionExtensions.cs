using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting
{
    internal static class CollectionExtensions
    {
        internal static IEnumerable<ElementInfo> GetListItems(this IList list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var j = i;
                yield return new ElementInfo(i.ToString(), list[i].GetType(), l => ((IList) l)[j]);
            }
        }
    }
}