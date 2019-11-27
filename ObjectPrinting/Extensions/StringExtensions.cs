using System;

namespace ObjectPrinting.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException($"{nameof(maxLength)} {maxLength} was negative");

            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length < maxLength ? value : value.Substring(0, maxLength);
        }
    }
}