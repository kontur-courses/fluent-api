using System;
using System.Globalization;

namespace ObjectPrinting;

public static class TypePrintingExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, T>(
        this TypePrintingConfig<TOwner, T> config,
        CultureInfo cultureInfo) where T : IFormattable
    {
        var serializerRules = config.Serializer.SerializerRules;
        serializerRules[typeof(T)] = x => string.Format(cultureInfo, "{0}", x);
        
        return config.HeadConfig;
    }
}