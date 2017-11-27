using System.Globalization;

namespace ObjectPrinting.Solved
{
	public static class PropertyPrintingConfigExtensions
	{
		public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, 
            int maxLen)
		{
		    IPropertyPrintingConfig<TOwner, string> propertyConfig = propConfig;

		    if (string.IsNullOrWhiteSpace(propertyConfig.PropName))
		        ((IPrintingConfig<TOwner>) propertyConfig.ParentConfig).SetAverageStringLength(maxLen);
		    else
                ((IPrintingConfig<TOwner>)propertyConfig.ParentConfig).SetStringPropertyLength(propertyConfig.PropName, maxLen);

            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, 
            CultureInfo cultureInfo)
        {
            ((IPrintingConfig<TOwner>) ((IPropertyPrintingConfig<TOwner, int>) propConfig).ParentConfig)
                .AddTypeCulture(typeof(int), cultureInfo);
            return ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {
            ((IPrintingConfig<TOwner>)((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig)
                .AddTypeCulture(typeof(double), cultureInfo);
            return ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig,
            CultureInfo cultureInfo)
        {
            ((IPrintingConfig<TOwner>)((IPropertyPrintingConfig<TOwner, long>)propConfig).ParentConfig)
                .AddTypeCulture(typeof(long), cultureInfo);
            return ((IPropertyPrintingConfig<TOwner, long>)propConfig).ParentConfig;
        }
    }
}