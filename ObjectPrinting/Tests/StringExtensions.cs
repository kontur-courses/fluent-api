namespace ObjectPrinting.Tests
{
    using System;
    using System.Collections.Generic;

    public static class StringExtensions
    {
        public static HashSet<string> ToHashSet(this string text, string delimiter)
        {
            return text.Split(new[] { delimiter }, StringSplitOptions.None)
                       .ToHashSet();
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> sequence)
        {
            var result = new HashSet<T>();
            foreach (var element in sequence)
                result.Add(element);

            return result;
        }
    }
}
