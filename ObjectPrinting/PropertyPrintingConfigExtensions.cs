using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, sbyte> propertyPrintingConfig,
    CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<sbyte>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, byte> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<byte>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<short>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ushort> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<ushort>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<int>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, uint> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<uint>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<long>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<ulong>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<float>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<double>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> propertyPrintingConfig,
            CultureInfo culture)
        {
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).SetCulture<decimal>(culture);
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
        {
            var propertyInfo = (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).PropertyInfo;
            var config = new PrintingConfig<TOwner>((propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).ParentConfig);

            if (propertyInfo == null)
                (config as IPrintingConfig).CustomTypesPrints[typeof(string)] = value => ((string)value).Substring(0, length);
            else
                (config as IPrintingConfig).CustomPropertiesPrints[propertyInfo] = value => ((string)value).Substring(0, length);
            return config;
        }
    }
}
