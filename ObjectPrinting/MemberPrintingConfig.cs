using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> parent;
    private readonly Dictionary<MemberInfo, Func<object, string>> customSerializers;
    private readonly MemberInfo memberInfo;

    public MemberPrintingConfig(PrintingConfig<TOwner> parent, 
        Dictionary<MemberInfo, Func<object, string>> customSerializers,
        MemberInfo memberInfo)
    {
        this.parent = parent;
        this.customSerializers = customSerializers;
        this.memberInfo = memberInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        customSerializers[memberInfo] = obj => print((TPropType)obj);
        return parent;
    }
}