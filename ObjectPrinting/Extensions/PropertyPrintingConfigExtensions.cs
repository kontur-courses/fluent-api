using System;
using System.Globalization;
using ObjectPrinting.PrintingConfigs;

namespace ObjectPrinting.Extensions;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLen);

        return propConfig.Using(s => s.Length > maxLen ? s.Substring(0, maxLen) : s);
    }
        
    public static PrintingConfig<TOwner> Using<TOwner, TProp>(
        this PropertyPrintingConfig<TOwner, TProp> propConfig, CultureInfo culture) where TProp : IFormattable
    {
        return propConfig.Using(value => value.ToString(null, culture));
    }
}