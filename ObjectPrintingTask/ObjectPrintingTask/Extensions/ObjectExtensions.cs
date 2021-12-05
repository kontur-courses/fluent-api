using System;

namespace ObjectPrintingTask.Extensions
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<Printer<T>, Printer<T>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config), "Config can not be null");

            var printingConfig = config(ObjectPrinter.For<T>());
            return printingConfig.PrintToString(obj);
        }
    }
}