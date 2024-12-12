using System;
using ObjectPrinting.Serializer.Configs;

namespace ObjectPrinting.Tools;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj)
        => ObjectPrinter.For<T>().PrintToString(obj);
    
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config) 
        => config(ObjectPrinter.For<T>()).PrintToString(obj);
}