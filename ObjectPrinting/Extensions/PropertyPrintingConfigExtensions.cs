using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig,
            CultureInfo culture)
            where TPropType : IFormattable
        {
            propConfig.PrintingConfig.AddCultureProperties<TPropType>(culture);

            return propConfig.Using(obj => obj.ToString(string.Empty, culture));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(str => str[..Math.Min(maxLen, str.Length)]);
        }
    }
}