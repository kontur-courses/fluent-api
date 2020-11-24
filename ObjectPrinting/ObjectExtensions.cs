using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return new ObjectPrinter<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return new ObjectPrinter<T>(config(PrintingConfig<T>.For<T>())).PrintToString(obj);
        }
    }
}