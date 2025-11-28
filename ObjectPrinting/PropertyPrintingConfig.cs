using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> _parentConfig;
    private readonly MemberInfo _memberInfo;

    public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo memberInfo)
    {
        _parentConfig = parentConfig;
        _memberInfo = memberInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        return _parentConfig.WithMemberSerializer(_memberInfo, serializer);
    }
}