using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfiguration<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> parentConfig;
    private readonly MemberInfo? propertyMemberInfo;

    internal PrintingConfig<TOwner> ParentConfig => parentConfig;
    internal MemberInfo? PropertyMemberInfo => propertyMemberInfo;

    public PropertyPrintingConfiguration(PrintingConfig<TOwner> parentConfig, MemberInfo? propertyMemberInfo)
    {
        this.parentConfig = parentConfig;
        this.propertyMemberInfo = propertyMemberInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> printingMethod)
    {
        if (propertyMemberInfo == null)
        {
            parentConfig.AddTypeSerializer(printingMethod);
        }
        else
        {
            parentConfig.AddPropertySerializer(propertyMemberInfo.Name, printingMethod);
        }

        return parentConfig;
    }
}