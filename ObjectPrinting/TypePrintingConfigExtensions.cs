using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            var parentConfig = ((ITypePrintingConfig<TOwner, int>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(int), culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, long> propConfig, CultureInfo culture)
        {
            var parentConfig = ((ITypePrintingConfig<TOwner, long>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(long), culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            var parentConfig = ((ITypePrintingConfig<TOwner, double>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(double), culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, decimal> propConfig, CultureInfo culture)
        {
            var parentConfig = ((ITypePrintingConfig<TOwner, decimal>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(decimal), culture);
            return parentConfig;
        }
    }
}