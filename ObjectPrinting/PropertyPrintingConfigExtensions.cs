using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T objectToPrint, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(objectToPrint);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            return propConfig.Using(str => str.Substring(0, Math.Min(maxLength, str.Length)));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IMemberPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            return propConfig.Using(str => str.Substring(0, Math.Min(maxLength, str.Length)));
        }
        
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> config,
            CultureInfo cultureInfo, int precision = 0) where TPropType : struct, IFormattable
        {
            return config.Using(arg => arg.ToString($"n{precision}", cultureInfo));
        }
    }
}