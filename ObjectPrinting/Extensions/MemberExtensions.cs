using System;
using System.Globalization;
using ObjectPrinting.MemberConfigurator;
using ObjectPrinting.ObjectConfiguration;

namespace ObjectPrinting.Extensions;

public static class MemberExtensions
{
    public static IObjectConfiguration<TOwner> TrimByLength<TOwner>(
        this IMemberConfigurator<TOwner, string> propertyConfig, int length) =>
        propertyConfig.Configure(item => item.ToString()?[..length] + Environment.NewLine);

    public static IObjectConfiguration<TOwner> SetCulture<TOwner, T>(this IMemberConfigurator<TOwner, T> propertyConfig,
        CultureInfo cultureInfo)
        where T : IFormattable => propertyConfig.Configure(item => ((T)item).ToString(null, cultureInfo));
}