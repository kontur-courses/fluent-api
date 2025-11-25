using System;

namespace ObjectPrinting;

public static class ObjectPrinterExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        return ObjectPrinter.For<T>().PrintToString(obj);
    }

    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configBuilder)
    {
        var config = configBuilder(ObjectPrinter.For<T>());
        return config.PrintToString(obj);
    }
}