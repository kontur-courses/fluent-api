using System;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TPropType> : IInnerPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    private readonly MemberInfo selectedMember;

    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember)
    {
        this.printingConfig = printingConfig;
        this.selectedMember = selectedMember;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        printingConfig.AddCustomMemberSerializer(selectedMember, serializer);
        return printingConfig;
    }
}