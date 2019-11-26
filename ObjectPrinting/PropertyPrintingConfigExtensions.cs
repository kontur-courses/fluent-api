using System;

namespace ObjectPrinting
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
            return propConfig.Using(str => TrimToLength(str, maxLen));
        }

        private static string TrimToLength(string str, int maxLen)
        {
            if (str.Length > maxLen)
                str = str.Substring(0, maxLen);
            return str;
        }
    }
}