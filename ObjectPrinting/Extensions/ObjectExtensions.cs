using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj) => ObjectPrinter.For<T>().Build().PrintToString(obj);

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configure) 
            => configure(ObjectPrinter.For<T>()).Build().PrintToString(obj);
    }
}