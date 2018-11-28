using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToStr<T>(this T obj)
            => ObjectPrinter.For<T>().PrintToString(obj);

        public static string PrintToStr<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> func)
            => func(ObjectPrinter.For<T>()).PrintToString(obj);
    }
}