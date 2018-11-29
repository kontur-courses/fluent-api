using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this IMemberPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(e => e.Substring(0, maxLen < e.Length ? maxLen : e.Length));
        }

    }
}