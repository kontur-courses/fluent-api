using System.Globalization;

namespace ObjectPrinting
{

    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, int> typePrintingConfig, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, double> typePrintingConfig, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
        }
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, float> typePrintingConfig, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
        }
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this TypePrintingConfig<TOwner, string> pc, int length)
        {
            return ((ITypePrintingConfig<TOwner>)pc).PrintingConfig;
        }
    }
}