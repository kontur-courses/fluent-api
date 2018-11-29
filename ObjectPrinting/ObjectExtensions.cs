using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string Serialize<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
        public static string Serialize<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}