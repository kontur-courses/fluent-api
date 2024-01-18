using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (maxLen < 0)
            {
                throw new ArgumentException("Error: Negative trimming length is not allowed");
            }
            return propConfig.Using(x => x.Length > maxLen ? x[..maxLen] : x);
        }
    }
}
