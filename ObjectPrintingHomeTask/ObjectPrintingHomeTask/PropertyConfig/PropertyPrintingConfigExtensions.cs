using System;
using ObjectPrintingHomeTask.Config;
using ObjectPrintingHomeTask.ObjectPrinting;

namespace ObjectPrintingHomeTask.PropertyConfig
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
            => config(ObjectPrinter.For<T>()).PrintToString(obj);

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>
            (this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
            => propConfig.Using((inputString) => inputString.Substring(0, maxLen));
    }
}
