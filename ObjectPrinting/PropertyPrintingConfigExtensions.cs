using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PropertyPrintingConfig<TOwner, string> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> config, int length)
    {
        return config.Using(x => x[..length]);
    }

    public static PropertyPrintingConfig<TOwner, TProperty> Format<TOwner, TProperty>(
        this PropertyPrintingConfig<TOwner, TProperty> config, CultureInfo cultureInfo, string? format = null)
        where TProperty : IFormattable
    {
        return config.Using(x => x.ToString(format, cultureInfo));
    }
}