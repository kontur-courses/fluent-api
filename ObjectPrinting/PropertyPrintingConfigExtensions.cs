using System;
using System.Linq;
using ObjectPrinting.Serializers;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> MaxLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig,
        int maxLength)
    {
        var headConfig = propertyPrintingConfig.HeadConfig;
        
        ((MembersSerializerByMember)headConfig.MembersSerializers
            .First(x => x is MembersSerializerByMember))
            .SerializerRules[propertyPrintingConfig.PropertyInfo] = x => TrimString(x, maxLength);
        
        return headConfig;
    }

    private static string TrimString(object value, int maxLength)
    {
        var propertyString = (value as string)!;
        return propertyString[..Math.Min(propertyString.Length, maxLength)];
    }
}