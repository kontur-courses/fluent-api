using System;
using ObjectPrinting.Abstractions.Configs;

namespace ObjectPrinting;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        return ObjectPrinter.For<T>().PrintToString(obj);
    }

    public static string PrintToString<T>(this T obj, Func<IPrintingConfig<T>, IPrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }
}