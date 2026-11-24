using System;
using System.Reflection;
using ObjectPrinting.Configs;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropertyType>(
    PrintingConfig<TOwner> printingConfig,
    MemberInfo? member = null)
{
    public PrintingConfig<TOwner> Using(Func<TPropertyType, string> print)
    {
        if (member == null)
            printingConfig.RegisterTypeFormatter(print);

        else
            printingConfig.RegisterMemberFormatter(member, print);

        return printingConfig;
    }
}