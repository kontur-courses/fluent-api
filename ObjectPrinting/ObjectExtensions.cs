using System;

namespace ObjectPrinting;

public static class ObjectPrinterExtensions
{
    public static string PrintToString<T>(this T obj, int? nestingLevel = null)
    {
        return ObjectPrinter.For<T>().PrintToString(obj, nestingLevel);
    }
    
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configurator, int? nestingLevel = null)
    {
        return configurator(ObjectPrinter.For<T>()).PrintToString(obj, nestingLevel);
    }
}