using System;

namespace ObjectPrinting
{
    public static class StringExtensions
    {
        public static string TrimLineTerminator(this string line) => line.TrimEnd(Environment.NewLine.ToCharArray());
    }
}