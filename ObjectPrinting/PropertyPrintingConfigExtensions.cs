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
            var propertyInfo = (propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).PropertyInfo;
            var config = new PrintingConfig<TOwner>((propertyPrintingConfig as IPropertyPrintingConfig<TOwner>).ParentConfig);

            if (propertyInfo == null)
                (config as IPrintingConfig).CustomTypesPrints[typeof(string)] = value => ((string)value).Substring(0, length);
            else
                (config as IPrintingConfig).CustomPropertiesPrints[propertyInfo.Name] = value => ((string)value).Substring(0, length);
            return config;
        }
    }
}
