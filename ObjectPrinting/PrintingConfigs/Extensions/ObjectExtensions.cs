using System;

namespace ObjectPrinting.PrintingConfigs.Extensions;

public static class ObjectExtensions
{
    public static string Print<T>(this T obj)
    {
        return ObjectPrinter.For<T>().Create().PrintToString(obj);
    }
    
    public static string Print<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).Create().PrintToString(obj);
    }
}

