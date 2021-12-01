using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Extensions
{
    public static class EnumerableExtensions
    {
        public static string GetStringValueOrDefault(this IEnumerable enumerable, string indent)
        {
            var sb = new StringBuilder(Environment.NewLine);

            if (enumerable == null)
                sb.AppendLine("null");
            else
            {
                foreach (var element in enumerable)
                    sb.AppendLine($"{indent}{element}");
            }

            return sb.ToString();
        }

        public static string GetStringValueOrDefault(this IEnumerable<IEnumerable> nestingEnumerable,
            string indentForNested, string indentForElements)
        {
            var sb = new StringBuilder(Environment.NewLine);

            if (nestingEnumerable == null)
                sb.AppendLine("null");
            else
            {
                foreach (var enumerable in nestingEnumerable)
                {
                    if (enumerable == null)
                    {
                        sb.AppendLine($"{indentForNested}null");
                        continue;
                    }

                    sb.AppendLine($"{indentForNested}IEnumerable");

                    foreach (var element in enumerable)
                        sb.AppendLine($"{indentForElements}{element}");
                }
            }

            return sb.ToString();
        }
    }
}