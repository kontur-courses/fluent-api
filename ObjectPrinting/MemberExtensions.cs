using System.Globalization;

namespace ObjectPrinting.Solved;

public static class MemberExtensions
{
    public static IBasicConfigurator<TOwner> TrimByLength<TOwner>(
        this IMemberConfigurator<TOwner, string> propertyConfig, int length)
    {
        return propertyConfig.BasicConfigurator;
    }

    public static IBasicConfigurator<TOwner> SetCulture<TOwner>(this IMemberConfigurator<TOwner, double> propertyConfig,
        CultureInfo cultureInfo)
    {
        return propertyConfig.Configure(cultureInfo);
    }
}