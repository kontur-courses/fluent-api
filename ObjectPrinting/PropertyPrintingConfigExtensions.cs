using ObjectPrinting.Solved;
using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyConfig, int length)
    {
        return propertyConfig.ParentConfig;
    }

    public static PrintingConfig<TOwner> Using<TOwner, TType>(
        this PropertyPrintingConfig<TOwner, TType> propertyConfig, CultureInfo culture)
        where TType : IFormattable
    {
        return propertyConfig.ParentConfig;
    }
}
