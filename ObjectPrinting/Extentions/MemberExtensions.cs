using System;
using System.Globalization;
using ObjectPrinting.MemberConfigurator;
using ObjectPrinting.ObjectConfiguration;

namespace ObjectPrinting.Extentions;

public static class MemberExtensions
{
    public static IObjectConfiguration<TOwner> TrimByLength<TOwner>(
        this IMemberConfigurator<TOwner, string> propertyConfig, int length) =>
        propertyConfig.Configure(s => s.ToString()?[..length] + Environment.NewLine);

    public static IObjectConfiguration<TOwner> SetCulture<TOwner>(this IMemberConfigurator<TOwner, int> propertyConfig,
        CultureInfo cultureInfo) => propertyConfig.Configure(s =>
    {
        var item = int.Parse(s.ToString() ?? string.Empty);
        return item.ToString(cultureInfo);
    });
}