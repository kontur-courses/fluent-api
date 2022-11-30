using System;
using System.Data;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj) where T : notnull
        {
            if (typeof(T) == typeof(CustomSerializablePrintingConfig<>))
                throw new ConstraintException();
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
        
        public static string PrintToString<T>(this T obj, Func<CustomSerializablePrintingConfig<T>, CustomSerializablePrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}