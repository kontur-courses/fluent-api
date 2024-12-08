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
    
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this TypePrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        return propConfig.Using(s => s[..maxLen]);
    }
}