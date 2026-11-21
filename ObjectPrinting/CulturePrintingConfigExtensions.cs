using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class CulturePrintingConfigExtensions
    {
        public static PrintingConfig<TRoot> Using<TRoot, TValue>(
            this TypePrintingConfig<TRoot, TValue> config,
            CultureInfo culture)
            where TValue : IFormattable
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(culture);

            return config.UsingCulture(culture);
        }

        public static PrintingConfig<TRoot> Using<TRoot, TValue>(
            this PropertyPrintingConfig<TRoot, TValue> config,
            CultureInfo culture)
            where TValue : IFormattable
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(culture);

            return config.UsingCulture(culture);
        }
    }
}