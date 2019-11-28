using System;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().GetStringRepresentation(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> func)
        {
            return func(ObjectPrinter.For<T>()).GetStringRepresentation(obj);
        }
    }
}