using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropertyType>(PrintingConfig<TOwner> printingConfig, MemberInfo? member = null)
{
    public PrintingConfig<TOwner> Using(Func<TPropertyType, string> print)
    {
        if (member == null)
            printingConfig.RegisterFormatter(print);

        else
            printingConfig.RegisterFormatterByMember(member, print);
        
        return printingConfig;
    }
}