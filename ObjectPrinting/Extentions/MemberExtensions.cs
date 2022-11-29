using System.Globalization;
using ObjectPrinting.BasicConfigurator;
using ObjectPrinting.MemberConfigurator;

namespace ObjectPrinting.Extentions;

public static class MemberExtensions
{
    public static IBasicConfigurator<TOwner> TrimByLength<TOwner>(
        this IMemberConfigurator<TOwner, string> propertyConfig, int length) =>
        propertyConfig.Configure(length);

    public static IBasicConfigurator<TOwner> SetCulture<TOwner>(this IMemberConfigurator<TOwner, int> propertyConfig,
        CultureInfo cultureInfo) =>
        propertyConfig.Configure(cultureInfo);
}