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

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentException("Max length is negative!");

            var trimmer = new Func<string, string>(value
                => value.Length > maxLen
                ? value[..maxLen]
                : value);

            return propConfig.Using(trimmer);
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
            where TPropType : IFormattable
        {
            return propConfig.Using(value => value.ToString(null, culture));
        }
    }
}