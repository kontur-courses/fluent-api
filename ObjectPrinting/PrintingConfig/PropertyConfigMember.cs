using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectPrinting.Solved;

namespace ObjectPrinting.PrintingConfig;

public class PropertyConfigMember<TOwner, TPropType>(
    PrintingConfig<TOwner> parentConfig,
    Dictionary<MemberInfo, Func<object, string>> memberDictionary, MemberInfo memberInfo)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    internal readonly MemberInfo MemberInfo = memberInfo;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
    {
        Func<object, string> funcObj = o => func((TPropType)o);

        memberDictionary.Add(MemberInfo, funcObj);
        return ParentConfig;
    }
    
    
    public PrintingConfig<TOwner> ParentConfig { get; } = parentConfig;
}