using System;

namespace ObjectPrinting;

public static class ObjectSerializingExtensions
{
    public static string PrintToString<T>(this T obj) => 
        ObjectPrinter.For<T>().PrintToString(obj);

    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> printConfig) => 
        printConfig(ObjectPrinter.For<T>()).PrintToString(obj);
}