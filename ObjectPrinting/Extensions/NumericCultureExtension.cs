using System;
using System.Globalization;

namespace ObjectPrinting.Extensions;

public static class NumericCultureExtension
{
    public static PrintingConfig<TOwner> UseCulture<TOwner, TNumericType>(
        this PrintingConfiguration<TOwner, TNumericType> propertyStringConfiguration,
        CultureInfo culture) where TNumericType : IFormattable
    {
        propertyStringConfiguration.ParentConfig.AddNumericCulture<TNumericType>(culture);

        return propertyStringConfiguration.ParentConfig;
    }
}