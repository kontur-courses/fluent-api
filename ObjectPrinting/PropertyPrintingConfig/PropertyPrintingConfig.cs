﻿using System.Reflection;
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
            printingConfig.TypeSerializationMethod.Add(typeof(TPropType), p => print((TPropType)p));
        else
            printingConfig.MemberSerializationMethod.Add(memberInfo, p => print((TPropType)p));

        return printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}