using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
        {
            return new PrintingConfig<TOwner>(propertyPrintingConfig.Using(x => x[..Math.Min(length, x.Length)]));
        }
    }
}