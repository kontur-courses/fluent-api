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
            return propConfig.Using(x =>  x[..Math.Min(x.Length, maxLen)]);
        }
    }
}