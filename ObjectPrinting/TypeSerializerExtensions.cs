using System;
using System.Globalization;

namespace ObjectPrinting;

public static class TypeSerializerExtensions
{
    private static PrintingConfig<TOwner> GetConfig<TOwner, TParam>(ITypeSerializer<TParam, TOwner> typeSerializer)
    {
        return ((TypeSerializerImpl<TParam, TOwner>)typeSerializer).Config;
    }

    public static PrintingConfig<TOwner> UseCulture<TOwner, TParam>(
        this ITypeSerializer<TParam, TOwner> typeSerializer,
        CultureInfo cultureInfo)
    where TParam : IFormattable
    {
        if (cultureInfo == null)
            throw new ArgumentNullException($"{nameof(cultureInfo)} cannot be null");
        var config = GetConfig(typeSerializer);
        config.AddCultureSpec(typeof(TParam), cultureInfo);
        return config;
    }

    public static PrintingConfig<TOwner> UseMaxLength<TOwner>(
        this ITypeSerializer<string, TOwner> typeSerializer,
        int maxLength)
    {
        var config = GetConfig(typeSerializer);
        config.MaxStringLength = maxLength;
        return config;
    }
}