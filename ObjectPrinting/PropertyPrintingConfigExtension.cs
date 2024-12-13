using System;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtension
{
    public static PrintingConfig<TOwner> TrimTo<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
        int maxLength)
    {
        if (maxLength < 1)
            throw new ArgumentException($"{nameof(maxLength)} should be greater than 1");

        var parentConfig = config.parentConfig;

        parentConfig.AddPropertyMaxLenght(config.propertyInfo, maxLength);
        return parentConfig;
    }
}