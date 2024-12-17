using System;
using System.Globalization;

namespace ObjectPrinting.Extensions;

public static class CultureExtension
{
    public static PrintingConfig<TOwner> UseCulture<TOwner, TPropType>(
        this PropertyPrintingConfiguration<TOwner, TPropType> propertyStringConfiguration,
        CultureInfo culture) where TPropType : IFormattable
    {
        propertyStringConfiguration.ParentConfig.AddNumericCulture<TPropType>(culture);

        return propertyStringConfiguration.ParentConfig;
    }
}