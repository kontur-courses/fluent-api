using System.Globalization;
using System.Reflection;
using ObjectPrinting.ObjectConfiguration;
using ObjectPrinting.ObjectConfiguration.Implementation;

namespace ObjectPrinting.MemberConfigurator.Implementation;

public class MemberConfigurator<TOwner, T> : IMemberConfigurator<TOwner, T>
{
    private ObjectConfiguration<TOwner> ObjectConfiguration { get; }
    
    private readonly MemberInfo memberInfo;
    
    public MemberConfigurator(ObjectConfiguration<TOwner> objectConfiguration, MemberInfo memberInfo)
    {
        ObjectConfiguration = objectConfiguration;
        this.memberInfo = memberInfo;
    }
    
    public IObjectConfiguration<TOwner> Configure(CultureInfo cultureInfo)
    {
        if (!ObjectConfiguration.MemberInfoConfigs.ContainsKey(memberInfo))
            ObjectConfiguration.MemberInfoConfigs.Add(memberInfo, new UniversalConfig());
        
        ObjectConfiguration.MemberInfoConfigs[memberInfo].CultureInfo = cultureInfo;
        return ObjectConfiguration;
    }

    public IObjectConfiguration<TOwner> Configure(int length)
    {
        if (!ObjectConfiguration.MemberInfoConfigs.ContainsKey(memberInfo))
            ObjectConfiguration.MemberInfoConfigs.Add(memberInfo, new UniversalConfig());
        
        ObjectConfiguration.MemberInfoConfigs[memberInfo].TrimLength = length;
        return ObjectConfiguration;
    }
}