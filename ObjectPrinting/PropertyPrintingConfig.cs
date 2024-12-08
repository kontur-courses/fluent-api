using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    private readonly Dictionary<MemberInfo, Func<object, string>> serializers;
    private readonly MemberInfo memberInfo;

    public PropertyPrintingConfig(
        PrintingConfig<TOwner> printingConfig, 
        Dictionary<MemberInfo, Func<object, string>> serializers,
        MemberInfo memberInfo)
    {
        if (printingConfig == null || serializers == null || memberInfo == null)
            throw new ArgumentNullException();

        this.printingConfig = printingConfig;
        this.serializers = serializers;
        this.memberInfo = memberInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        if (serializer == null)
            throw new ArgumentNullException();

        serializers[memberInfo] = x => serializer((TPropType) x);
        return printingConfig;
    }
}