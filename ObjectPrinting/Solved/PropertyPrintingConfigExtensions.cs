using System;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (maxLen < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxLen), maxLen, "Maximum length must be non-negative.");
            }

            return propConfig.Using(value =>
                value is null
                    ? null
                    : value.Length > maxLen ? value.Substring(0, maxLen) : value);
        }
    }
}