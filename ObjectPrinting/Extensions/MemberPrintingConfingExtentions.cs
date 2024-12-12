using System;

namespace ObjectPrinting.Extensions;

public static class MemberPrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, IPrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static IPrintingConfig<TOwner> TrimmedToLength<TOwner>(this MemberPrintingConfig<TOwner, string> propConfig,
        int maxLen)
    {
        return ((IMemberPrintingConfig<TOwner, string>)propConfig).Config;
    }
}