using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(property => property.Substring(0, Math.Min(maxLen, property.Length)).ToString());
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, DateTime> propConfig, CultureInfo culture)
        {
            return propConfig.Using(x => x.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            return propConfig.Using(x => x.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            return propConfig.Using(x => x.ToString(culture));
        }
    }
}
