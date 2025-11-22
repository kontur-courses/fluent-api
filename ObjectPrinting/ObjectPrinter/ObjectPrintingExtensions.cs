using System;

namespace ObjectPrinting.ObjectPrinter;

public static class ObjectPrintingExtensions
{
    public static string PrintToString<T>(this T obj)
        => ObjectPrinter.For<T>().PrintToString(obj);

    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        => config(ObjectPrinter.For<T>()).PrintToString(obj);
}