using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this TypePrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, string>)propConfig).ParentConfig;

            parentConfig.AddSerializer(typeof(string), s => s[..maxLen]);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, T>(this TypePrintingConfig<TOwner, T> propConfig,
            CultureInfo culture)
            where T : IFormattable
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, T>)propConfig).ParentConfig;

            parentConfig.SetCulture(typeof(T), culture);

            return parentConfig;
        }
    }
}