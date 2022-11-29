using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace ObjectPrinting;

public class MemberConfigurator<TOwner, T> : IMemberConfigurator<TOwner, T>
{
    private readonly MemberInfo memberInfo;
    
    public MemberConfigurator(IBasicConfigurator<TOwner> basicConfigurator, MemberInfo memberInfo)
    {
        BasicConfigurator = basicConfigurator;
        this.memberInfo = memberInfo;
    }
    
    public IBasicConfigurator<TOwner> BasicConfigurator { get; }
    
    public IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo)
    {
        if (!BasicConfigurator.MemberInfoConfigs.ContainsKey(memberInfo))
            BasicConfigurator.MemberInfoConfigs.Add(memberInfo, new UniversalConfig());
        
        BasicConfigurator.MemberInfoConfigs[memberInfo].CultureInfo = cultureInfo;
        return BasicConfigurator;
    }

    public IBasicConfigurator<TOwner> Configure(int length)
    {
        if (!BasicConfigurator.MemberInfoConfigs.ContainsKey(memberInfo))
            BasicConfigurator.MemberInfoConfigs.Add(memberInfo, new UniversalConfig());
        
        BasicConfigurator.MemberInfoConfigs[memberInfo].TrimLength = length;
        return BasicConfigurator;
    }
}