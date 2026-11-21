using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxLen);

            return propConfig.Using(x => x is null
                ? "null"
                : x.Length > maxLen ? x[..maxLen] : x);
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig,
            CultureInfo culture)
            where TPropType : IFormattable
        {
            return propConfig.Using(x => x.ToString(null, culture));
        }

    }
}
