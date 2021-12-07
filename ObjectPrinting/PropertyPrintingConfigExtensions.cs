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
            propConfig.Using(s => s[..Math.Min(maxLen, s.Length - 1)]);
            return (propConfig).PrintingConfig;
        }

    }
}