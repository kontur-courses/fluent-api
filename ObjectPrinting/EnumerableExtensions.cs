using System;
using System.Collections;
using System.Text;

namespace ObjectPrinting
{
    public static class EnumerableExtensions
    {
        public static string GetStringValueOrDefault(this IEnumerable enumerable, string indent)
        {
            var sb = new StringBuilder(Environment.NewLine);
            foreach (var element in enumerable) sb.AppendLine($"{indent}{element}");

            return sb.ToString();
        }
    }
}