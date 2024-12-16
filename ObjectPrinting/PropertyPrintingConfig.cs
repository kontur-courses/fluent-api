using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    private readonly MemberInfo memberInfo;

    public PropertyPrintingConfig(
        PrintingConfig<TOwner> printingConfig, 
        MemberInfo memberInfo)
    {
        if (printingConfig == null || memberInfo == null)
            throw new ArgumentNullException();

        this.printingConfig = printingConfig;
        this.memberInfo = memberInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        if (serializer == null)
            throw new ArgumentNullException();

        printingConfig.UpdateSerializer(memberInfo, x => serializer((TPropType) x));
        return printingConfig;
    }
}