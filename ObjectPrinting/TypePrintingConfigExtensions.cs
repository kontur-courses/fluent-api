using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtension
    {
        public static PrintingConfig<TOwner> Use<TOwner, TType>
            (this TypePrintingConfig<TOwner, TType> typeConfig, CultureInfo culture) where TType : IFormattable

        {
            var configInterface = typeConfig as ITypePrintingConfig<TOwner, TType>;
            var parentInterface = configInterface.ParentConfig as IPrintingConfig<TOwner>;

            if (parentInterface.TypeCultures.ContainsKey(typeof(TType)))
                parentInterface.TypeCultures[typeof(TType)] = culture;
            else 
                parentInterface.TypeCultures.Add(typeof(TType), culture);

            return (PrintingConfig<TOwner>) parentInterface;
        }
    }
}