using System;

namespace ObjectPrinting;

public static class ObjectPrinterExtensions
{
    public static string PrintToString<T>(
        this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        var cfg = config(new PrintingConfig<T>());
        return cfg.PrintToString(obj);
    }
}