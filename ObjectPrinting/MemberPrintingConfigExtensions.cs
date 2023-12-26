using System;
using System.Globalization;

namespace ObjectPrinting;

public static class MemberPrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this MemberPrintingConfig<TOwner, string> memberConfig, int maxLen)
    {
        if (maxLen < 0)
            throw new ArgumentException("Length to trim must be non negative");

        var memberInfo = memberConfig.MemberInfo;

        if (memberInfo == null)
            memberConfig.PrintingConfig.TrimStringValue = maxLen;
        else
            memberConfig.PrintingConfig.TrimmedMembers[memberInfo] = maxLen;

        return memberConfig.PrintingConfig;
    }

    public static PrintingConfig<TOwner> WithCulture<TOwner, TMemberType>(
        this MemberPrintingConfig<TOwner, TMemberType> memberConfig, CultureInfo culture)
        where TMemberType : IFormattable
    {
        memberConfig.PrintingConfig.CulturesForTypes[typeof(TMemberType)] = culture;

        return memberConfig.PrintingConfig;
    }
}