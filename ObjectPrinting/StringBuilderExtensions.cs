using System.Text;

namespace ObjectPrinting
{
    internal static class StringBuilderExtensions
    {
        internal static StringBuilder Append(this StringBuilder sb, params string[] source)
        {
            foreach (var el in source)
                sb.Append(el);
            return sb;
        }
    }
}