using System;
using System.Linq;
using System.Reflection;
using ObjectPrinting.Serializers;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TProperty>(PrintingConfig<TOwner> headConfig, MemberInfo memberInfo)
{
    internal PrintingConfig<TOwner> HeadConfig => headConfig;
    internal MemberInfo MemberInfo => memberInfo;
    internal readonly MembersSerializerByMember Serializer =
        headConfig.MembersSerializers.OfType<MembersSerializerByMember>().First();
    
    public PrintingConfig<TOwner> Using(Func<TProperty, string> printing)
    {
        Serializer.SerializerRules[MemberInfo] = p => printing((TProperty)p);
        
        return HeadConfig;
    }
}