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
            if (maxLen < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLen));


            return propConfig.Using(value =>
                value == null || value.Length <= maxLen
                    ? value
                    : value.Substring(0, maxLen));
        }
    }
}