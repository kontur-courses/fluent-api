using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj, int maxNestingLevel)
        {
            return ObjectPrinter.For<T>().PrintToString(obj, maxNestingLevel);
        }

        public static string PrintToString<T>(this ICollection<T> obj, int maxNestingLevel)
        {
            var result = new StringBuilder();
            result.Append("["+ Environment.NewLine);
            foreach (var t in obj)
            {
                result.Append(ObjectPrinter.For<T>().PrintToString(t, maxNestingLevel));
                result.Append(", " + Environment.NewLine);
            }
            result.Append("]");
            return result.ToString();
        }
    }
}