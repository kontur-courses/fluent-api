using System;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T? obj)
    {
        return ObjectPrinter.For<T>().PrintToString(obj);
    }

    public static string PrintToString<T>(this T? obj, Func<PrintingConfig<T>, PrintingConfig<T>> configurator)
    {
        var config = configurator(ObjectPrinter.For<T>());
        return config.PrintToString(obj);
    }
}