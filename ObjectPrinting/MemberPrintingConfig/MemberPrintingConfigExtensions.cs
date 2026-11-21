using System;
using System.Globalization;
using System.Numerics;

namespace ObjectPrinting;

public static class MemberPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Truncate<TOwner>(
        this MemberPrintingConfig<TOwner, string> config,
        int maxLength)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        return config.WithMaxLength(maxLength);
    }

    public static PrintingConfig<TOwner> Using<TOwner, TProp>(
        this MemberPrintingConfig<TOwner, TProp> config,
        CultureInfo culture)
        where TProp : INumber<TProp>
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(culture);

        return config.WithCulture(culture);
    }
}