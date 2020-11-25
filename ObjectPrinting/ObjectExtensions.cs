using System;

namespace ObjectPrinting
{
    internal static class ObjectExtensions
    {
        internal static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        internal static string PrintToString<T>(this T obj,
            Func<PrintingConfig<T>, PrintingConfig<T>> configPrinter)
        {
            var printer = configPrinter(ObjectPrinter.For<T>());
            return printer.PrintToString(obj);
        }
    }
}