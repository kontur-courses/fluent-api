using System;
using System.Globalization;

namespace ObjectPrinting.Solved;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyConfig,
        int maxLength)
    {
        propertyConfig.AlternativePrint = str => str[..maxLength];
        return propertyConfig.ParentConfig;
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> propertyConfig,
        CultureInfo culture) where TPropType : IFormattable
    {
        propertyConfig.AlternativePrint = property => string.Format(culture, "{0}", property);
        return propertyConfig.ParentConfig;
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> propertyConfig,
        Func<TPropType, string> print)
    {
        propertyConfig.AlternativePrint = print;
        return propertyConfig.ParentConfig;
    }
}