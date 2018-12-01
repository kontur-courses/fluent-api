using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public static class StringExtensions
    {
        public static HashSet<string> ToHashSet(this string text, string delimiter) =>
            new HashSet<string>(text.Split(new[] {delimiter}, StringSplitOptions.None));
    }
}
