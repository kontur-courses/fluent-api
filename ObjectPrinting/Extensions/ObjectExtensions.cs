using System;

namespace ObjectPrinting.Extensions;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        return ObjectPrinter.For<T>().PrintToString(obj);
    }
    
    public static string PrintToString<TOwner>(this TOwner obj,
        Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
    {
        return config(new PrintingConfig<TOwner>())
            .PrintToString(obj);
    }
}