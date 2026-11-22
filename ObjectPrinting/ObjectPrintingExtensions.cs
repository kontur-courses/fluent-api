using System;

namespace ObjectPrinting;

public static class ObjectPrintingExtensions
{
    public static string PrintToString<TOwner>(this TOwner obj)
    {
        return new PrintingConfig<TOwner>().PrintToString(obj);
    }
    
    public static string PrintToString<TOwner>(
        this TOwner obj,
        Action<PrintingConfig<TOwner>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var config = new PrintingConfig<TOwner>();
        configure(config);

        return config.PrintToString(obj);
    }
    
}