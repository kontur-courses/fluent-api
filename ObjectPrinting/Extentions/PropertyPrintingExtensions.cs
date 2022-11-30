using System;
using ObjectPrinting.BasicConfigurator.Implementation;

namespace ObjectPrinting.Extentions;

public static class PropertyPrintingExtensions
{
    public static string PrintToString<T>(this T item) =>
        ObjectConfig.For<T>().ConfigurePrinter().PrintToString(item);
    
    public static string PrintToString<T>(this T obj, Func<ObjectConfigurator<T>, PrintingConfig<T>> config) =>
        config(new ObjectConfigurator<T>()).PrintToString(obj);
}