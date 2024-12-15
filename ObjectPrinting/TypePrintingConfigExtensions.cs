using System;

namespace ObjectPrinting;

public static class TypePrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TType>(this ITypePrintingConfig<TOwner, TType> typeConfig,
        IFormatProvider formatProvider) where TType : IFormattable
    {
        typeConfig.ParentConfig.Culture = formatProvider;
        return typeConfig.ParentConfig;
    }
}