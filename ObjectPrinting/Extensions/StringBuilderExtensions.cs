using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendCollection<T>(this StringBuilder sb, IEnumerable<T> items)
        {
            foreach (var item in items)
                sb.Append(item);

            return sb;
        }
    }
}