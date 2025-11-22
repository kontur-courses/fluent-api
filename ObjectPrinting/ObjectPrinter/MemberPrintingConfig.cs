using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.ObjectPrinter;

public class MemberPrintingConfig<TOwner, TMemberType>(
    PrintingConfig<TOwner> printingConfig,
    MemberInfo? memberInfo = null)
{
    protected readonly PrintingConfig<TOwner> PrintingConfig = printingConfig;
    private readonly MemberInfo? MemberInfo = memberInfo;

    public PrintingConfig<TOwner> Using(Func<TMemberType, string> printingMethod)
    {
        if (MemberInfo is null)
            PrintingConfig.CustomTypeSerializers[typeof(TMemberType)] = Wrapper;
        else
            PrintingConfig.CustomMemberSerializers[MemberInfo] = Wrapper;

        return PrintingConfig;

        string Wrapper(object value) => printingMethod((TMemberType)value);
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        PrintingConfig.CulturesForTypes[typeof(TMemberType)] = culture;
        return PrintingConfig;
    }

    public PrintingConfig<TOwner> TrimmedToLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (MemberInfo is null)
            PrintingConfig.TrimmedTypes[typeof(TMemberType)] = length;
        else
            PrintingConfig.TrimmedMembers[MemberInfo] = length;

        return PrintingConfig;
    }
}