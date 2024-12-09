using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Solved;

public class PropertyConfigMember<TOwner,TPropType>: IPropertyPrintingConfig<TOwner, TPropType>
{
    private readonly Dictionary<MemberInfo, Func<object, string>> memberDictionary;
    private readonly MemberInfo memberInfo;

    public PropertyConfigMember(PrintingConfig<TOwner> parentConfig, Dictionary<MemberInfo,
        Func<object,string>> memberDictionary,
        MemberInfo memberInfo)
    {
        this.memberDictionary = memberDictionary;
        this.memberInfo = memberInfo;
        ParentConfig = parentConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
    {
        Func<object, string> funcObj = o => func((TPropType)o);  
        
        memberDictionary.Add(memberInfo, funcObj);
        return ParentConfig;
    }
    public PrintingConfig<TOwner> ParentConfig { get; }
}