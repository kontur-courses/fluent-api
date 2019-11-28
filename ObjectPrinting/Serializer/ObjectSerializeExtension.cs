using System;
using ObjectPrinting.PrintingConfigs;

namespace ObjectPrinting
{
    public static class ObjectSerializeExtension
    {
        public static PrintingConfig<T> Serialize<T>(this T obj)
        {
            return ObjectPrinter.For<T>();
        }
        
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
        
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}