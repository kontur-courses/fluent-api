using System;
using System.Globalization;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TPropertyType>(
        this PropertyPrintingConfig<TOwner, TPropertyType> propertyPrintingConfig,
        CultureInfo cultureInfo)
        where TPropertyType : IFormattable
    {
        return propertyPrintingConfig.Using(value => value.ToString(null, cultureInfo));
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
    {
        return propertyPrintingConfig.Using(value => value.Length > length ? value.Remove(length) : value);
    }
}