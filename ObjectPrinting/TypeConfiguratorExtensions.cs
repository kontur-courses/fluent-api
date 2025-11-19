using System;
using System.Globalization;

namespace ObjectPrinting;

public static class TypeConfiguratorExtensions
{
    public static PrintingConfig<TOwner> WithCulture<TOwner, T>(
        this TypeConfigurator<TOwner, T> configurator,
        CultureInfo culture) where T : struct, IFormattable
    {
        return configurator.WithSerialization(value => value.ToString(null, culture));
    }

    public static PrintingConfig<TOwner> Trim<TOwner>(
        this TypeConfigurator<TOwner, string> configurator,
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