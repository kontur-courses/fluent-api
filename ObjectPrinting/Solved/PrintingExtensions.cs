using System;
using ObjectPrinting.Solved.Interfaces;

namespace ObjectPrinting.Solved
{
    public static class PrintingExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this ITypePrinting<TOwner, string> config, int maxLen)
        {
            if (maxLen < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxLen), maxLen, "Maximum length must be non-negative.");
            }

            return config.Using(value =>
                value is null
                    ? null
                    : value.Length > maxLen ? value.Substring(0, maxLen) : value);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IPropertyPrinting<TOwner, string> config, int maxLen)
        {
            if (maxLen < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxLen), maxLen, "Maximum length must be non-negative.");
            }

            return config.Using(value =>
                value is null
                    ? null
                    : value.Length > maxLen ? value.Substring(0, maxLen) : value);
        }
    }
}