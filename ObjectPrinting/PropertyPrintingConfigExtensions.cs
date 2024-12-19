using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner, TPropType>
            (this IPropertyPrintingConfig<TOwner, TPropType> config,
            CultureInfo culture)
            where TPropType : IFormattable
        {
            return config.Using(x => x.ToString(null, culture));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner,
            string> propConfig, int maxLen)
        {
            return propConfig.Using(x =>
                (x.Length > maxLen)
                ? x[..maxLen]
                : x);
        }
    }
}