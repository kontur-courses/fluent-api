using System;
using System.Linq;

namespace ObjectPrinting
{
    public static class StringExtensions
    {
        public static string RemoveEmptyLines(this string str)
        {
            return string.Join("\r", str.Split(Environment.NewLine[0]).Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}