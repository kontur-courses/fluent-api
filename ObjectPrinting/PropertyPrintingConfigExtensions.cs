using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config) =>
            config(ObjectPrinter.For<T>())
                .PrintToString(obj);

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config,
            int maxLen) =>
            config.Using(s => s.Substring(0, Math.Min(maxLen, s.Length)));

        public static PrintingConfig<TOwner> Using<TOwner, T>(
            this PropertyPrintingConfig<TOwner, T> config,
            CultureInfo culture) where T : IFormattable =>
            config.Using(i => i.ToString("", culture));
    }
}
