using System;
using ObjectPrinting.PrintingConfigs;

namespace ObjectPrinting.Extensions
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
                throw new ArgumentOutOfRangeException(nameof(maxLen));

            return propConfig.Using(s => s.Length > maxLen ? s.Substring(0, maxLen) : s);
        }
    }
}