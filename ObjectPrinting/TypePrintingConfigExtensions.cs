using System.Globalization;


namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> config, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this TypePrintingConfig<TOwner, string> config, int length)
        {
            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }
    }
}
