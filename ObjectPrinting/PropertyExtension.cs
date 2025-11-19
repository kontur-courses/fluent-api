using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyExtensions
{
    public static PrintingConfig<TOwner> WithCulture<TOwner, TProp>(
        this PropertyConfigurator<TOwner, TProp> configurator,
        CultureInfo culture) where TProp : struct, IFormattable
    {
        configurator.WithInternalCulture(culture);
        return configurator.WithSerialization(value => value.ToString(null, culture));
    }

    public static PrintingConfig<TOwner> Trim<TOwner>(
        this PropertyConfigurator<TOwner, string> configurator,
        int start, int end)
    {
        return configurator.WithSerialization(str =>
        {
            if (str == null) return "null";
            var length = Math.Min(str.Length - start, end - start);
            return length > 0 ? str.Substring(start, length) : string.Empty;
        });
    }
}