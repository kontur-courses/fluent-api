using System;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TMemberType>(
    PrintingConfig<TOwner> printingConfig, MemberInfo? memberInfo = null)
    : IMemberPrintingConfig<TOwner>
{
    internal readonly PrintingConfig<TOwner> PrintingConfig = printingConfig; 
    internal readonly MemberInfo? MemberInfo = memberInfo;
    
    public PrintingConfig<TOwner> Using(Func<TMemberType, string> printingMethod)
    {
        var resultDelegate = new Func<object, string>(obj => printingMethod((TMemberType)obj));
        if (MemberInfo is null)
            PrintingConfig.CustomTypeSerializers[typeof(TMemberType)] = resultDelegate;
        else
            PrintingConfig.CustomMemberSerializers[MemberInfo] = resultDelegate;

        return PrintingConfig;
    }

    PrintingConfig<TOwner> IMemberPrintingConfig<TOwner>.ParentConfig => PrintingConfig;
}

public interface IMemberPrintingConfig<TOwner>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}