using System;
using ObjectPrinting.ObjectConfiguration.Implementation;

namespace ObjectPrinting.Extentions;

public static class PropertyPrintingExtensions
{
    public static string PrintToString<T>(this T item) =>
        ObjectConfig.For<T>().Build().PrintToString(item);
    
    public static string PrintToString<T>(this T obj, Func<ObjectConfiguration<T>, PrintingConfig<T>> config) =>
        config(new ObjectConfiguration<T>()).PrintToString(obj);
}