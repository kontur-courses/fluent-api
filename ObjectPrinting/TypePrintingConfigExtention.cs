using System.Globalization;
using System.Runtime.CompilerServices;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtention
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this TypePrintingConfig<TOwner, TPropType> self, CultureInfo culture)
        {
            var printingConfig = (self as ITypePrintingConfig<TOwner>).PrintingConfig;
            var typeName = (self as ITypePrintingConfig<TOwner>).TypeName;
            (printingConfig as IPrintingConfig).Cultures.Add(typeName, culture);
            return (self as ITypePrintingConfig<TOwner>).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Trimming<TOwner>(this TypePrintingConfig<TOwner, string> self, int trimmedStringLenght)
        {
            var printingConfig = (self as ITypePrintingConfig<TOwner>).PrintingConfig;
            var typeName = (self as ITypePrintingConfig<TOwner>).TypeName;
            (printingConfig as IPrintingConfig).TrimLenghts.Add(typeName, trimmedStringLenght);
            return (self as ITypePrintingConfig<TOwner>).PrintingConfig;
        }
    }
}
