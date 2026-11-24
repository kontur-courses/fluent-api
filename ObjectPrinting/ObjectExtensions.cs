using System;
using ObjectPrinting.Configs;

namespace ObjectPrinting;

public static class ObjectExtensions
{
    public static string PrintToString<TOwner>(this TOwner obj)
    {
        var printer = ObjectPrinter.For<TOwner>();
        return printer.PrintToString(obj);
    }

    public static string PrintToString<TOwner>(this TOwner obj,
        Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
    {
        return config(ObjectPrinter.For<TOwner>()).PrintToString(obj);
    }
}