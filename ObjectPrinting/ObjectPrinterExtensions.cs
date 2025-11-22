using System;
using ObjectPrinting.Configs;

namespace ObjectPrinting;

public static class ObjectPrinterExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        return ObjectPrinter.InClass<T>().PrintToString(obj);
    }

    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        var printer = config(ObjectPrinter.InClass<T>());
        return printer.PrintToString(obj);
    }
}