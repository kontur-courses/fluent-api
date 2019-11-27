using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>(this TypePrintingConfig<TOwner, TType> typeConfig, CultureInfo culture)
            where TType : IFormattable
        {
            var printingConfig = ((ITypePrintingConfig<TOwner, TType>)typeConfig).ParentConfig;
            ((IPrintingConfig<TOwner>)printingConfig).TypeCultureInfo[typeof(TType)] = culture;
            return printingConfig;
        }
    }
}
