using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static string PrintToString<T>(this T? obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        var propertyInfo = propConfig.PropertyInfo;
        propConfig.PrintingConfig.TrimmedProperties[propertyInfo] = maxLen;

        return propConfig.PrintingConfig;
    }

    public static PrintingConfig<TOwner> WithCulture<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> propertyConfig, CultureInfo culture)
        where TPropType : IFormattable
    {
        propertyConfig.PrintingConfig.CulturesForTypes[typeof(TPropType)] = culture;

        return propertyConfig.PrintingConfig;
    }
}