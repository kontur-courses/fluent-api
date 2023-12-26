using System;

namespace ObjectPrinting.Extensions
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
        
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>,PrintingConfig<T>> func)
        {
            return func(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}