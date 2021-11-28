using System;
using System.Globalization;
using ObjectPrinting.Solved.PrintingConfiguration;

namespace ObjectPrinting.Solved.Extensions
{
    public static class ChildPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            var printingConfig = config(ObjectPrinter.For<T>());
            return printingConfig.PrintToString(obj);
        }

        public static PrintingConfig<TOwner> Using<TOwner, TMemberType>(
            this IChildPrintingConfig<TOwner, TMemberType> config,
            CultureInfo cultureInfo = null,
            string format = null) where TMemberType : IFormattable
        {
            return config.Using(obj => obj.ToString(format, cultureInfo));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IChildPrintingConfig<TOwner, string> config, int maxLen)
        {
            return config.Using(str => str.Substring(0, Math.Min(str.Length, maxLen)));
        }
    }
}