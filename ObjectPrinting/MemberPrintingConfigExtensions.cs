using System;

namespace ObjectPrinting;

public static class MemberPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> MaxLength<TOwner>(
        this MemberPrintingConfig<TOwner, string> memberPrintingConfig,
        int maxLength)
    {
        var serializerRules = memberPrintingConfig.Serializer.SerializerRules;
        serializerRules[memberPrintingConfig.MemberInfo] = x => TrimString(x, maxLength);
        
        return memberPrintingConfig.HeadConfig;
    }

    private static string TrimString(object value, int maxLength)
    {
        var propertyString = (value as string)!;
        return propertyString[..Math.Min(propertyString.Length, maxLength)];
    }
}