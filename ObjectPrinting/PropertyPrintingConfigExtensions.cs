using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyConfig, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException("Длина строки должна быть целым неотрицательным числом");

        return propertyConfig.Using(value =>
            value.Length <= length ? value : value[..length]);
    }

    public static PrintingConfig<TOwner> Using<TOwner, TType>(
        this PropertyPrintingConfig<TOwner, TType> propertyConfig, CultureInfo culture)
        where TType : IFormattable
    {
        var member = propertyConfig.Member;
        var rule = propertyConfig.ParentConfig.GetOrCreateRule<TType>(member);

        rule.Culture = culture;
        return propertyConfig.ParentConfig;
    }
}
