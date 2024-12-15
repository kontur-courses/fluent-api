using System;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        => config(ObjectPrinter.For<T>()).PrintToString(obj);

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this IPropertyPrintingConfig<TOwner, string> propConfig,
        int maxLen)
    {
        propConfig.ParentConfig.MaxLengths.TryAdd(propConfig.PropertyName, maxLen);
        return propConfig.ParentConfig;
    }
}