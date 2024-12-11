using System;

namespace ObjectPrinting.Extensions;

public static class ObjectExtensions
{
    public static string PrintToString<TOwner>(this TOwner obj)
        => ObjectPrinter<TOwner>.Configure().PrintToString(obj);

    public static string PrintToString<TOwner>(this TOwner obj,
        Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
    {
        return config(ObjectPrinter<TOwner>.Configure())
            .PrintToString(obj);
    }
}