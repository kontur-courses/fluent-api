using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this PropertyPrintingConfig<TOwner, TPropType> propConfig, 
        CultureInfo culture) where TPropType : IFormattable
    {
        return propConfig.Using(value => value.ToString(null, culture));
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        if (maxLen < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLen), "Length must be non-negative");
        }

        return propConfig.Using(value => value.Length > maxLen 
            ? value[..maxLen] 
            : value);
    }
}