using System;
using ObjectPrinting.Solved;

namespace ObjectPrinting;

public static class PropertyPrintingExtensions
{
    public static string PrintToString<T>(this T item) =>
        ObjectPrinter.For<T>().PrintToString(item);
    
    public static string PrintToString<T>(this T obj, Func<ObjectConfigurator<T>, PrintingConfig<T>> config) =>
        config(new ObjectConfigurator<T>()).PrintToString(obj);
}