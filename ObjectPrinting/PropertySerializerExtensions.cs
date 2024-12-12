using System;

namespace ObjectPrinting;

public static class PropertySerializerExtensions
{
    public static PrintingConfig<TOwner> UseMaxLength<TOwner>(
        this IPropertySerializer<TOwner, string> serializer,
        int maxLength)
    {
        if (maxLength < 0)
            throw new ArgumentOutOfRangeException($"{nameof(maxLength)} cannot be negative");
        var typeSerializer = (PropertySerializerImpl<TOwner, string>)serializer;
        var config = typeSerializer.Config;
        config.AddStringPropertyLength(typeSerializer.MemberInfo, maxLength);
        return config;
    }
}