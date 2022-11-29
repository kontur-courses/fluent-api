using System.Globalization;

namespace ObjectPrinting;

public static class MemberExtensions
{
    public static IBasicConfigurator<TOwner> TrimByLength<TOwner>(
        this IMemberConfigurator<TOwner, string> propertyConfig, int length)
    {
        return propertyConfig.Configure(length);
    }

    public static IBasicConfigurator<TOwner> SetCulture<TOwner>(this IMemberConfigurator<TOwner, int> propertyConfig,
        CultureInfo cultureInfo)
    {
        return propertyConfig.Configure(cultureInfo);
    }
}