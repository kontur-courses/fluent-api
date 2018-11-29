using System.Globalization;
using System.Runtime.CompilerServices;


namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypePrintingConfig<TOwner, int> config, CultureInfo culture)
        {
            ((ITypePrintingConfig<TOwner>)config).PrintingConfig
                .Settings.SetCultureInfoForType(typeof(int), culture);

            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypePrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            ((ITypePrintingConfig<TOwner>)config).PrintingConfig
                .Settings.SetCultureInfoForType(typeof(double), culture);

            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypePrintingConfig<TOwner, long> config, CultureInfo culture)
        {
            ((ITypePrintingConfig<TOwner>)config).PrintingConfig
                .Settings.SetCultureInfoForType(typeof(long), culture);

            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this TypePrintingConfig<TOwner, string> config, int length)
        {
            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }
    }
}
