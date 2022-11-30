using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj) => ObjectPrinter.For<T>().PrintToString(obj);

        public static string PrintListToString<T>(this List<T> obj) => ObjectPrinter.For<T>().PrintListToString(obj);

        public static string PrintingWithConfigure<T>(this T obj,
            Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static string PrintingListWithConfigure<T>(this List<T> obj,
            Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintListToString(obj);
        }
    }
}