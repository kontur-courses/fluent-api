using System;
using System.Globalization;
using ObjectPrinting.Serializer.Configs;
using ObjectPrinting.Serializer.Configs.Children;

namespace ObjectPrinting.Tools;

public static class PropertyConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this IChildConfig<TOwner, string> propConfig, int maxLen)
    {
        return propConfig.ParentConfig;
    }

    public static PrintingConfig<TOwner> WithCulture<TOwner, TPropType>(
        this IChildConfig<TOwner, TPropType> propConfig, CultureInfo culture) where TPropType : IFormattable
    {
        propConfig.ParentConfig.CulturesForTypes[typeof(TPropType)] = culture;
        return propConfig.ParentConfig;
    }
}