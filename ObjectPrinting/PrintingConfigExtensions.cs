using System;

namespace ObjectPrinting;

public static class PrintingConfigExtensions
{
    public static string PrintToString<TOwner>(this TOwner obj)
    {
        return ObjectPrinter.For<TOwner>().PrintToString(obj);
    }

    public static string PrintToString<TOwner>(this TOwner obj, Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> configFunc)
    {
        var config = new PrintingConfig<TOwner>();
        configFunc(config);
        return config.PrintToString(obj);
    }
}