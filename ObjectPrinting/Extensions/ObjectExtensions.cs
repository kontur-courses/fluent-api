using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().Build().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configure)
        {
            return configure(ObjectPrinter.For<T>()).Build().PrintToString(obj);
        }
    }
}