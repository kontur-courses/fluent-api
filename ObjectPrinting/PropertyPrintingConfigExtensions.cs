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
            config.PrintingConfig.Config.TypeCultures[typeof(TPropType)] = culture;
        else
            config.PrintingConfig.Config.SetCultureForPropertyFromExpression(config.propertySelector, culture);
        return config.PrintingConfig;
    }

    public static PrintingConfig<TOwner> Cut<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int maxLen)
    {
        if (config.propertySelector is null)
            config.PrintingConfig.Config.SetStringMaxLen(maxLen);
        else
            config.PrintingConfig.Config
                .SetMaxLenForPropertyFromExpression(config.propertySelector, maxLen);
        return config.PrintingConfig;
    }
}
