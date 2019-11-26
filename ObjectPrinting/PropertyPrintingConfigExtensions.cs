using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig, CultureInfo currentCulture)
        {
            var config = new PrintingConfig<TOwner>((propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).ParentConfig);
            (config as IPrintingConfig).CustomTypesPrints.Add(typeof(int), value => currentCulture.ToString());
            return config;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
        {
            var propertyFullName = (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).PropertyFullName;
            var config = new PrintingConfig<TOwner>((propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).ParentConfig);

            if (propertyFullName == null)
                (config as IPrintingConfig).CustomTypesPrints[typeof(string)] = value => ((string)value).Substring(0, length);
            else
                (config as IPrintingConfig).CustomPropertiesPrints[propertyFullName] = value => ((string)value).Substring(0, length);
            return config;
        }
    }
}
