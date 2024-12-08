using System;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TMemberType>(
    PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo = null)
    : IMemberPrintingConfig<TOwner, TMemberType>
{
    internal readonly PrintingConfig<TOwner> PrintingConfig = printingConfig; 
    internal readonly MemberInfo? MemberInfo = memberInfo;
    
    public PrintingConfig<TOwner> Using(Func<TMemberType, string> printingMethod)
    {
        if (MemberInfo is null)
            PrintingConfig.CustomTypeSerializers[typeof(TMemberType)] = printingMethod;
        else
            PrintingConfig.CustomMemberSerializers[MemberInfo] = printingMethod;

        return PrintingConfig;
    }

    PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TMemberType>.ParentConfig => PrintingConfig;
}

public interface IMemberPrintingConfig<TOwner, TMemberType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}