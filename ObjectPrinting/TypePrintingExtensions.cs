using System;
using System.Globalization;
using System.Linq;
using ObjectPrinting.Serializers;

namespace ObjectPrinting;

public static class TypePrintingExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, T>(
        this TypePrintingConfig<TOwner, T> config,
        CultureInfo cultureInfo) where T : IFormattable
    {
        var headConfig = config.HeadConfig;
        
        ((MembersSerializerByType)headConfig.MembersSerializers
            .First(x => x is MembersSerializerByType))
            .SerializerRules[typeof(T)] = x => string.Format(cultureInfo, "{0}", x);
        
        return headConfig;
    }
}