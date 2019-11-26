using System;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.SetUp<T>()).PrintToString(obj);
        }
    }
}