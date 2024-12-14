using System;
using System.Linq;
using System.Reflection;
using ObjectPrinting.Serializers;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TProperty>(PrintingConfig<TOwner> headConfig, MemberInfo memberInfo)
{
    public PrintingConfig<TOwner> HeadConfig => headConfig;
    public MemberInfo PropertyInfo => memberInfo;
    
    public PrintingConfig<TOwner> Using(Func<TProperty, string> printing)
    {
        ((MembersSerializerByMember)HeadConfig.MembersSerializers
            .First(x => x is MembersSerializerByMember))
            .SerializerRules[PropertyInfo] = p => printing((TProperty)p);
        
        return HeadConfig;
    }
}