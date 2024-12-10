using System;
using ObjectPrinting.Configs;
using ObjectPrinting.Configs.Children;

namespace ObjectPrinting.Tools;

public static class PropertyConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this IChildrenConfig<TOwner, string> propConfig, int maxLen)
    {
        return propConfig.ParentConfig;
    }

}