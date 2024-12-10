using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.PropertyPrintingConfig;

public class PropertyPrintingConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> printingConfig,
    MemberInfo? memberInfo = null)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    public MemberInfo PropertyMemberInfo =>
        memberInfo != null ? memberInfo : throw new InvalidOperationException("MemberInfo is null.");

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (memberInfo is null)
            printingConfig.TypeSerializationMethod.AddMethod(typeof(TPropType), print);
        else
            printingConfig.MemberSerializationMethod.AddMethod(memberInfo, print);

        return printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}