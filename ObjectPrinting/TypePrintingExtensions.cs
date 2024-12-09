using System;
using System.Globalization;

namespace ObjectPrinting;

public static class TypePrintingExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, T>(
        this TypePrintingConfig<TOwner, T> config,
        CultureInfo cultureInfo) where T : IFormattable
    {
        var headConfig = config.HeadConfig;
        
        headConfig.Cultures[typeof(T)] = cultureInfo;
        
        return headConfig;
    }
}