using System.Globalization;


namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypePrintingConfig<TOwner, int> config, CultureInfo culture)
        {
               ((ISettings)((ITypePrintingConfig<TOwner>)config).PrintingConfig)
                .Settings.SetCultureInfoForType(typeof(int), culture);

            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypePrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            ((ISettings)((ITypePrintingConfig<TOwner>)config).PrintingConfig)
                .Settings.SetCultureInfoForType(typeof(double), culture);

            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypePrintingConfig<TOwner, long> config, CultureInfo culture)
        {
            ((ISettings)((ITypePrintingConfig<TOwner>)config).PrintingConfig)
                .Settings.SetCultureInfoForType(typeof(long), culture);

            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypePrintingConfig<TOwner, float> config, CultureInfo culture)
        {
            ((ISettings)((ITypePrintingConfig<TOwner>)config).PrintingConfig)
                .Settings.SetCultureInfoForType(typeof(float), culture);

            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }
    }
}
