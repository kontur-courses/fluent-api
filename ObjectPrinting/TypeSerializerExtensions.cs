using System;
using System.Globalization;

namespace ObjectPrinting;

public static class TypeSerializerExtensions
{
    public static PrintingConfig<TOwner> UseCulture<TOwner>(
        this ITypeSerializer<double, TOwner> typeSerializer,
        CultureInfo cultureInfo)
    {
        var config = ((ITypeSerializerInternal<TOwner>)typeSerializer).Config;
        config.DoubleCultureInfo = cultureInfo;
        return config;
    }

    public static PrintingConfig<TOwner> UseCulture<TOwner>(
        this ITypeSerializer<float, TOwner> typeSerializer,
        CultureInfo cultureInfo)
    {
        var config = ((ITypeSerializerInternal<TOwner>)typeSerializer).Config;
        config.FloatCultureInfo = cultureInfo;
        return config;
    }
}