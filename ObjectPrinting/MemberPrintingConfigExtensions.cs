using System;

namespace ObjectPrinting
{
    public static class MemberPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this MemberPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(str => str[..Math.Min(maxLen, str.Length)]);
        }
    }
}