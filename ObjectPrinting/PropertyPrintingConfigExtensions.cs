using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        #region Using CultureInfo for numeric types

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

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> propPrintingConfig,
            CultureInfo culture)
        {
            ((IPropertyPrintingConfig<TOwner, long>)propPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(long)] = culture;

            return ((IPropertyPrintingConfig<TOwner, long>)propPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, short> propPrintingConfig,
            CultureInfo culture)
        {
            ((IPropertyPrintingConfig<TOwner, short>)propPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(short)] = culture;

            return ((IPropertyPrintingConfig<TOwner, short>)propPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, decimal> propPrintingConfig,
            CultureInfo culture)
        {
            ((IPropertyPrintingConfig<TOwner, decimal>)propPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(decimal)] = culture;

            return ((IPropertyPrintingConfig<TOwner, decimal>)propPrintingConfig).PrintingConfig;
        }

        #endregion

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