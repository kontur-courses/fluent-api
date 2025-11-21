using System;
using System.Globalization;

namespace ObjectPrinting.Solved;

public static class PropertyPrintingConfigExtensions
{
    public static string? PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        IPropertyPrintingConfig<TOwner, string> config = propConfig;
        var parent = config.ParentConfig;
        var memberInfo = config.MemberInfo;

        parent.AddStringTrimming(memberInfo, maxLen);
        return parent;
    }
    
    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this IPropertyPrintingConfig<TOwner, TPropType> config,
        CultureInfo culture)
        where TPropType : IFormattable
    {
        if (config.MemberInfo == null)
            config.ParentConfig.AddTypeCulture<TPropType>(culture);
        else
            config.ParentConfig.AddMemberCulture(config.MemberInfo, culture);

        return config.ParentConfig;
    }
}