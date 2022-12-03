using System;
using System.Globalization;

namespace ObjectPrinting.Configuration
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<IPrintingConfig<T>, PrintingConfig<T>> config)
        {
            return new ObjectPrinter<T>(config(new PrintingConfig<T>())).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(r => !string.IsNullOrEmpty(r) && r.Length > maxLen ? r[..maxLen] : r);
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture) where TPropType : IFormattable
        {
            return propConfig.Using(r => r.ToString(null, culture));
        }
    }
}