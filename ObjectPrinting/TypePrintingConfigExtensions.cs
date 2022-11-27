using System;
using System.Globalization;

namespace ObjectPrinting;

public static class TypePrintingConfigExtensions
{
    public static TypePrintingConfig<TOwner, string> Cut<TOwner>(
        this TypePrintingConfig<TOwner, string> config, int length)
    {
        return config.Serialize(x => x[..length]);
    }
    
    public static TypePrintingConfig<TOwner, TType> Format<TOwner, TType>(
        this TypePrintingConfig<TOwner, TType> config, CultureInfo? cultureInfo, string? format = null)
        where TType : IFormattable
    {
        return config.Serialize(x => x.ToString(format, cultureInfo));
    }

    public static PrintingConfig<TOwner> SpecifyCulture<TOwner>(this PrintingConfig<TOwner> printingConfig,
        CultureInfo cultureInfo)
    {
        return printingConfig.ForType<IFormattable>().SerializeAllAssignable(x => x.ToString(null, cultureInfo));
    }
}