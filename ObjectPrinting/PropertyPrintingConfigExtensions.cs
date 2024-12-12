using System;


namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyConfig,
        int maxLen)
    {
        propertyConfig.PrintingConfig.AddLengthOfProperty(propertyConfig.PropertyInfo, maxLen);
        return propertyConfig.PrintingConfig;
    }
}