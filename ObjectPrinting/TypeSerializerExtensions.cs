using System;
using System.Globalization;

namespace ObjectPrinting;

public static class TypeSerializerExtensions
{
    private static PrintingConfig<TOwner> GetConfig<TOwner, TParam>(ITypeSerializer<TParam, TOwner> typeSerializer)
    {
        return ((TypeSerializerImpl<TParam, TOwner>)typeSerializer).Config;
    }
    
    public static PrintingConfig<TOwner> UseCulture<TOwner>(
        this ITypeSerializer<double, TOwner> typeSerializer,
        CultureInfo cultureInfo)
    {
        if (cultureInfo == null)
            throw new ArgumentNullException($"{nameof(cultureInfo)} cannot be null");
        var config = GetConfig(typeSerializer);
        config.DoubleCultureInfo = cultureInfo;
        return config;
    }

    public static PrintingConfig<TOwner> UseCulture<TOwner>(
        this ITypeSerializer<float, TOwner> typeSerializer,
        CultureInfo cultureInfo)
    {
        if (cultureInfo == null)
            throw new ArgumentNullException($"{nameof(cultureInfo)} cannot be null");
        var config = GetConfig(typeSerializer);
        config.FloatCultureInfo = cultureInfo;
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