using System.Globalization;

namespace ObjectPrinting;

public static class TypeExtensions
{
    public static IBasicConfigurator<TOwner> TrimByLength<TOwner>(this ITypeConfigurator<TOwner, string> typeConfig, int length)
    {
        return typeConfig.Configure(length);
    }

    public static IBasicConfigurator<TOwner> SetCulture<TOwner>(this ITypeConfigurator<TOwner, int> typeConfig,
        CultureInfo cultureInfo)
    {
        return typeConfig.Configure(cultureInfo);
    }
}