using System;
using ObjectPrinting.PrintingConfiguration;

namespace ObjectPrinting.Extensions
{
    public static class PrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static PrintingConfig<TOwner> SetLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
            int maxLen)
        {
            propConfig.PrintingConfig.SetMaxLength(propConfig.PropertyInfo, maxLen);
            return propConfig.PrintingConfig;
        }
    }
}