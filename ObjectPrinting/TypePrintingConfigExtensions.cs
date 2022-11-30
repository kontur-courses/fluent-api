using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this TypePrintingConfig<TOwner, string> typeConfig, int maxLen)
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, string>)typeConfig).ParentConfig;

            parentConfig.AddSerializer(typeof(string), s => s[..maxLen]);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, T>(this TypePrintingConfig<TOwner, T> typeConfig,
            CultureInfo culture)
            where T : IFormattable
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, T>)typeConfig).ParentConfig;

            parentConfig.SetCulture(typeof(T), culture);

            return parentConfig;
        }
    }
}