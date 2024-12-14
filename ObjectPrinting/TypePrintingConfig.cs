using System;
using System.Linq;
using ObjectPrinting.Serializers;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropertyType>(PrintingConfig<TOwner> headConfig)
{
    public PrintingConfig<TOwner> HeadConfig { get; } = headConfig;
    
    public PrintingConfig<TOwner> Using(Func<TPropertyType, string> printing)
    {
        ((MembersSerializerByType)HeadConfig.MembersSerializers
            .First(x => x is MembersSerializerByType))
            .SerializerRules[typeof(TPropertyType)] = p => printing((TPropertyType)p);
        
        return HeadConfig;
    }
}

