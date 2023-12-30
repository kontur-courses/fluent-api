using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (maxLen < 0)
            {
                throw new ArgumentException("Error: The length of the truncated string cannot be negative");
            }
            return propConfig.Using(x => x.Length > maxLen ? x[..maxLen] : x);
        }
    }
}