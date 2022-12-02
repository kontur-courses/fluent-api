using System;
using System.Globalization;
using System.Numerics;

namespace ObjectPrinting;

public static class TypePrintingConfigExtensions
{
    public static TypePrintingConfig<TOwner, string> TrimmedToLength<TOwner>(
        this TypePrintingConfig<TOwner, string> config, int length)
    {
        return config.Using(x => x[..length]);
    }

    public static TypePrintingConfig<TOwner, TType> Using<TOwner, TType>(
        this TypePrintingConfig<TOwner, TType> config, CultureInfo cultureInfo)
        where TType : INumber<TType>, IFormattable
    {
        return config.Using(x => x.ToString(null, cultureInfo));
    }

    public static TypePrintingConfig<TOwner, TType> Format<TOwner, TType>(
        this TypePrintingConfig<TOwner, TType> config, CultureInfo? cultureInfo, string? format = null)
        where TType : IFormattable
    {
        return config.Using(x => x.ToString(format, cultureInfo));
    }

    public static PrintingConfig<TOwner> Using<TOwner>(this PrintingConfig<TOwner> printingConfig,
        CultureInfo cultureInfo)
    {
        return printingConfig.Printing<IFormattable>().UsingAllAssignable(x => x.ToString(null, cultureInfo));
    }
}