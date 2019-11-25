using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>
            (this TypePrintingConfig<TOwner, TType> typeConfig, CultureInfo culture) where TType : IFormattable
        
        {
            var parent = ((ITypePrintingConfig<TOwner, TType>) typeConfig).ParentConfig;
            ((IPrintingConfig<TOwner>)parent).TypeCultures.Add(typeof(TType), culture);
            return parent;
        }
    }
}