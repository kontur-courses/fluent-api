using System;
using System.Globalization;

namespace ObjectPrinting.Extensions;

public static class CultureExtension
{
    public static PrintingConfig<TOwner> UseCulture<TOwner, TPropType>(
        this PropertyPrintingConfiguration<TOwner, TPropType> propertyStringConfiguration,
        CultureInfo culture) where TPropType : IFormattable
    {
        propertyStringConfiguration.ParentConfig.AddTypeSerializer<TPropType>(t => t.ToString(null, culture));

        return propertyStringConfiguration.ParentConfig;
    }
}