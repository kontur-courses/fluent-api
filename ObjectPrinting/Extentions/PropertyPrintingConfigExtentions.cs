using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtentions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propertyPrintingConfig,
            CultureInfo cultureInfo) where TPropType : IFormattable
        {
            Func<TPropType, string> serializer = obj => obj.ToString(null, cultureInfo);
            return propertyPrintingConfig.Using(serializer);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int maxLen)
        {
            Func<string, string> serializer = str => str.Substring(0, Math.Min(str.Length, maxLen));
            return propertyPrintingConfig.Using(serializer);

        }
    }
}
