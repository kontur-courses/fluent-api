using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting;

public static class PrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Exclude<TOwner>(this PrintingConfig<TOwner> config, Type excludedType)
    {
        ArgumentNullException.ThrowIfNull(excludedType);
        config.ExcludeType(excludedType);
        return config;
    }

    public static PrintingConfig<TOwner> Exclude<TOwner>(
        this PrintingConfig<TOwner> config,
        IEnumerable<Type> excludedTypes)
    {
        ArgumentNullException.ThrowIfNull(excludedTypes);
        foreach (var excludedType in excludedTypes)
        {
            config.Exclude(excludedType);
        }

        return config;
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPrintingType>(
        this ValuePrintingConfig<TOwner, TPrintingType> config,
        CultureInfo culture)
        where TPrintingType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(culture);
        return config.Using(value => value.ToString(null, culture));
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPrintingType>(
        this ValuePrintingConfig<TOwner, TPrintingType> config,
        string format)
        where TPrintingType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(format);
        return config.Using(value => value.ToString(format, null));
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPrintingType>(
        this ValuePrintingConfig<TOwner, TPrintingType> config,
        string format,
        CultureInfo culture)
        where TPrintingType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(culture);
        return config.Using(value => value.ToString(format, culture));
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this ValuePrintingConfig<TOwner, string> config,
        int maxLength)
    {
        return config.TrimmedToLength(maxLength, string.Empty);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this ValuePrintingConfig<TOwner, string> config,
        int maxLength,
        string ending)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        ArgumentNullException.ThrowIfNull(ending);
        return config.Using(value => value.Length <= maxLength ? value : value[..maxLength] + ending);
    }
}