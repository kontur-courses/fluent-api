using System;
using System.Globalization;
using ObjectPrinting.Configurations;

namespace ObjectPrinting.Extensions;

public static class TypePrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TType>(
        this TypePrintingConfig<TOwner, TType> typeConfig, CultureInfo culture)
        where TType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(culture);
        typeConfig.Using(culture);
        return typeConfig.ParentConfig;
    }
}