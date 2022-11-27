using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PropertyPrintingConfig<TOwner, string> Cut<TOwner>(
        this PropertyPrintingConfig<TOwner, string> config, int length)
    {
        return config.Serialize(x => x[..length]);
    }

    public static PropertyPrintingConfig<TOwner, TProperty> Format<TOwner, TProperty>(
        this PropertyPrintingConfig<TOwner, TProperty> config, CultureInfo cultureInfo, string? format = null)
        where TProperty : IFormattable
    {
        return config.Serialize(x => x.ToString(format, cultureInfo));
    }


    public static TypePrintingConfig<TOwner, TType> Format<TOwner, TType>(
        this TypePrintingConfig<TOwner, TType> config, CultureInfo cultureInfo, string? format = null)
        where TType : IFormattable
    {
        return config.Serialize(x => x.ToString(format, cultureInfo));
    }
}