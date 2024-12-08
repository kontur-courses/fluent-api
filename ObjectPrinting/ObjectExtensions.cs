using System;

namespace ObjectPrinting;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        return ObjectPrinter.For<T>().PrintToString(obj);
    }

    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configurer)
    {
        var config = configurer(ObjectPrinter.For<T>());
        return config.PrintToString(obj);
    }
}