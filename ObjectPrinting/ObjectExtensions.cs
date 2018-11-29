using System;
using ObjectPrinting.Config;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            var printingConfig = config(new PrintingConfig<T>());

            return printingConfig.PrintToString(obj);
        }
    }
}