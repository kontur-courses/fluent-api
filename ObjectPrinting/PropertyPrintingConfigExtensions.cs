using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> propPrintingConfig,
            CultureInfo culture)
        {
            ((IPropertyPrintingConfig<TOwner, double>)propPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(double)] = culture;

            return ((IPropertyPrintingConfig<TOwner, double>)propPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> propPrintingConfig,
            CultureInfo culture)
        {
            ((IPropertyPrintingConfig<TOwner, int>)propPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(int)] = culture;

            return ((IPropertyPrintingConfig<TOwner, int>)propPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, float> propPrintingConfig,
            CultureInfo culture)
        {
            ((IPropertyPrintingConfig<TOwner, float>)propPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(float)] = culture;

            return ((IPropertyPrintingConfig<TOwner, float>)propPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propPrintingConfig,
            int value)
        {
            var extendedPropPrintingConfig = ((IPropertyPrintingConfig<TOwner, string>)propPrintingConfig);
            var config = extendedPropPrintingConfig.PrintingConfig;

            config.StringsTrimValues[extendedPropPrintingConfig.Property] = value;

            return config;
        }
    }
}