using System;

namespace ObjectPrinting
{
    public static class ObjectPrinterExtensions
    {
        public static string PrintToString<T>(this T obj) => ObjectPrinter.For<T>().PrintToString(obj);

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configFunc) => configFunc(ObjectPrinter.For<T>()).PrintToString(obj);
    }
}