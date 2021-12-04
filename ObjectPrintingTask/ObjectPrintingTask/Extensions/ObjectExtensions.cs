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
            if (config == null)
                throw new ArgumentException("Config can not be null");

            var printingConfig = config(ObjectPrinter.For<T>());
            return printingConfig.BuildConfig().PrintToString(obj);
        }
    }
}