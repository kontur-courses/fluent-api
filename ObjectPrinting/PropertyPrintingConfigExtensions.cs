using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            parentConfig.AddSerializer(propConfig.Member, s => s[..maxLen]);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, T>(this PropertyPrintingConfig<TOwner, T> propConfig,
            CultureInfo culture)
            where T : IFormattable
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, T>)propConfig).ParentConfig;

            parentConfig.SetCulture(propConfig.Member, culture);

            return parentConfig;
        }
    }
}