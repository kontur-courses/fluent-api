using System;

namespace ObjectPrinting
{
    public static class ObjectPrinterExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>()
                .PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Action<PrintingConfig<T>> configuration)
        {
            var result = ObjectPrinter.For<T>();
            configuration(result);

            return result.PrintToString(obj);
        }
    }
}