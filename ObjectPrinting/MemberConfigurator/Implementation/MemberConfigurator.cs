using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectPrinting.ObjectConfiguration;
using ObjectPrinting.ObjectConfiguration.Implementation;

namespace ObjectPrinting.MemberConfigurator.Implementation;

public class MemberConfigurator<TOwner, T> : IMemberConfigurator<TOwner, T>
{
    private readonly ObjectConfiguration<TOwner> objectConfiguration;
    
    private readonly MemberInfo memberInfo;
    
    public MemberConfigurator(ObjectConfiguration<TOwner> objectConfiguration, MemberInfo memberInfo)
    {
        this.objectConfiguration = objectConfiguration;
        this.memberInfo = memberInfo;
    }

    public IObjectConfiguration<TOwner> Configure(Func<string, string> func)
    {
        if (!objectConfiguration.MemberInfoConfigs.ContainsKey(memberInfo))
            objectConfiguration.MemberInfoConfigs.Add(memberInfo, new List<Func<string, string>>());

        objectConfiguration.MemberInfoConfigs[memberInfo].Add(func);
        return objectConfiguration;
    }
}