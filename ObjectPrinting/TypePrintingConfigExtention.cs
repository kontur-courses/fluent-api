using System.Globalization;
using System.Runtime.CompilerServices;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtention
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> me, CultureInfo culture)
        {
            var printingConfig = (me as ITypePrintingConfig<TOwner>).PrintingConfig;
            var typeName = (me as ITypePrintingConfig<TOwner>).TypeName;
            (printingConfig as IPrintingConfig).Cultures.Add(typeName, culture);
            return (me as ITypePrintingConfig<TOwner>).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Trimming<TOwner>(this TypePrintingConfig<TOwner, string> me, int len)
        {
            var printingConfig = (me as ITypePrintingConfig<TOwner>).PrintingConfig;
            var typeName = (me as ITypePrintingConfig<TOwner>).TypeName;
            (printingConfig as IPrintingConfig).TrimLenghts.Add(typeName, len);
            return (me as ITypePrintingConfig<TOwner>).PrintingConfig;
        }
    }
}
