using System;

namespace ObjectPrinting;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        return ObjectPrinter.Setup<T>().Apply().PrintToString(obj);
    }

    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.Setup<T>()).Apply().PrintToString(obj);
    }
}
