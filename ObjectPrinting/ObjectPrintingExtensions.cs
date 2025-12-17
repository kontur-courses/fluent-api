using System;

namespace ObjectPrinting;

public static class ObjectPrintingExtensions
{
    public static string PrintToString<TOwner>(this TOwner obj)
    {
        return ObjectPrinter.For<TOwner>().PrintToString(obj);
    }

    public static string PrintToString<TOwner>(this TOwner obj, Func<IPrintingConfig<TOwner>, IPrintingConfig<TOwner>> config)
    {
        return config(ObjectPrinter.For<TOwner>()).PrintToString(obj);
    }
}