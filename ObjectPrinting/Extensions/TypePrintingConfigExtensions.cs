using System;
using System.Globalization;
using ObjectPrinting.Configurations;

namespace ObjectPrinting.Extensions;

public static class TypePrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TType>(
        this TypePrintingConfig<TOwner, TType> config, CultureInfo culture)
        where TType : IFormattable
    {
        return config.Using<TType>(culture);
    }
}