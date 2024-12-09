using System;

namespace ObjectPrinting.Extensions;

public static class ObjectExtensions
{
    public static string PrintToString<TOwner>(this TOwner obj)
    {
        return ObjectPrinter.For<TOwner>().PrintToString(obj);
    }
    
    public static string PrintToString<TOwner>(this TOwner obj,
        Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
    {
        return config(ObjectPrinter.For<TOwner>())
            .PrintToString(obj);
    }
}