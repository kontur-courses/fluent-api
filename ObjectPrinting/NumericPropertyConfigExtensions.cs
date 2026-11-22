using System;
using System.Globalization;
using System.Numerics;

namespace ObjectPrinting;

public static class NumericPropertyConfigExtensions
{
    public static PrintingConfig<TOwner> SetNumericCulture<TOwner, TProperty>(
        this PropertyConfig<TOwner, TProperty> propertyConfig,
        CultureInfo cultureInfo)
        where TProperty : INumber<TProperty>
    {
        ArgumentNullException.ThrowIfNull(propertyConfig);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        propertyConfig.ObjectSerializer.SetNumericCulture(
            propertyConfig.MemberInfo,
            cultureInfo
        );

        return propertyConfig.PrintingConfig;
    }
}