using System;
using System.Globalization;

namespace ObjectPrinting.Extensions;

public static class PrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Printing<TOwner, TType>(
        this PrintingConfig<TOwner> config, CultureInfo culture)
        where TType : IFormattable
    {
        config.SetTypeCulture<TType>(culture);
        return config;
    }
}