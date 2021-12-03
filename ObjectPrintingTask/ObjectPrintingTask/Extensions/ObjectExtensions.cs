using ObjectPrintingTask.PrintingConfiguration;
using System;

namespace ObjectPrintingTask.Extensions
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().BuildConfig().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            var printingConfig = config(ObjectPrinter.For<T>());
            return printingConfig.BuildConfig().PrintToString(obj);
        }
    }
}