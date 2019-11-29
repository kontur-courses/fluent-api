using System;
using ObjectPrinting.Core.PrintingConfig;

namespace ObjectPrinting.Core.PropertyPrintingConfig
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
            {
                throw new ArgumentException("MaxLen should be non-negative");
            }

            return propConfig.Using(str =>
                str.Length > maxLen
                    ? str.Substring(0, maxLen)
                    : str);
        }
    }
}