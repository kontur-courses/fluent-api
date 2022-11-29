using System.Globalization;
using ObjectPrinting.BasicConfigurator;
using ObjectPrinting.TypeConfigurator;

namespace ObjectPrinting.Extentions;

public static class TypeExtensions
{
    public static IBasicConfigurator<TOwner> TrimByLength<TOwner>(this ITypeConfigurator<TOwner, string> typeConfig,
        int length) =>
        typeConfig.Configure(length);

    public static IBasicConfigurator<TOwner> SetCulture<TOwner>(this ITypeConfigurator<TOwner, int> typeConfig,
        CultureInfo cultureInfo) =>
        typeConfig.Configure(cultureInfo);
}