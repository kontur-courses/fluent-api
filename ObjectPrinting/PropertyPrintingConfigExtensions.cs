using System.Globalization;
using System.Numerics;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> WithCulture<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> config,
        CultureInfo culture)
        where TPropType : INumber<TPropType>
    {
        if (config.propertySelector is null)
            config.PrintingConfig.config.TypeCultures[typeof(TPropType)] = culture;
        else
            config.PrintingConfig.config.SetCultureForPropertyFromExpression(config.propertySelector, culture);
        return config.PrintingConfig;
    }

    public static PrintingConfig<TOwner> Cut<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int maxLen)
    {
        if (config.propertySelector is null)
            config.PrintingConfig.config.SetStringMaxLen(maxLen);
        else
            config.PrintingConfig.config
                .SetMaxLenForPropertyFromExpression(config.propertySelector, maxLen);
        return config.PrintingConfig;
    }
}
