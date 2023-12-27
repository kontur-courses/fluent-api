using System;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TMemberType>
{
    internal readonly PrintingConfig<TOwner> PrintingConfig;
    internal readonly MemberInfo MemberInfo;

    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo = null)
    {
        PrintingConfig = printingConfig;
        MemberInfo = memberInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TMemberType, string> printingMethod)
    {
        if (MemberInfo is null)
            PrintingConfig.CustomTypeSerializers[typeof(TMemberType)] = printingMethod;
        else
            PrintingConfig.CustomMemberSerializers[MemberInfo] = printingMethod;

        return PrintingConfig;
    }
}