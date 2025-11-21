using System;

namespace ObjectPrinting;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        var printingConfig = ObjectPrinter.For<T>();
        printingConfig = config(printingConfig);
        return printingConfig.PrintToString(obj);
    }
}