using System;
using System.Linq;
using ObjectPrinting.Serializers;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropertyType>(PrintingConfig<TOwner> headConfig)
{
    internal PrintingConfig<TOwner> HeadConfig { get; } = headConfig;
    internal readonly MembersSerializerByType Serializer = 
        headConfig.MembersSerializers.OfType<MembersSerializerByType>().First();
    
    public PrintingConfig<TOwner> Using(Func<TPropertyType, string> printing)
    {
        Serializer.SerializerRules[typeof(TPropertyType)] = p => printing((TPropertyType)p);
        
        return HeadConfig;
    }
}

