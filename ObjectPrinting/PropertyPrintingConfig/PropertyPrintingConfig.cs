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
            printingConfig.TypeSerializationMethod.Add(typeof(TPropType), print);
        else
            printingConfig.MemberSerializationMethod.Add(memberInfo, print);

        return printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}