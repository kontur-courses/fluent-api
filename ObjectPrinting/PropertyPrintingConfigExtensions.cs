using System.Globalization;

namespace ObjectPrinting
{
	public static class PropertyPrintingConfigExtensions
	{
		public static PrintingConfig<TOwner> TrimmedToLength<TOwner>
            (this PropertyPrintingConfig<TOwner, string> config, int maxLen)
		{
		    var propConfig = config as IPropertyPrintingConfig<TOwner, string>;
		    var parentConfig = propConfig.Parent as IPrintingConfig;
		    parentConfig.TrimmingLenth.Add(propConfig.SelectedProperty, maxLen);
		    return (PrintingConfig<TOwner>) parentConfig;
        }

	    private static PrintingConfig<TOwner> SetCultureForType<TOwner, TPropType>
	        (PropertyPrintingConfig<TOwner, TPropType> config, CultureInfo culture)
	    {
	        var propConfig = config as IPropertyPrintingConfig<TOwner, TPropType>;
	        var parentConfig = propConfig.Parent as IPrintingConfig;
            parentConfig.NumericCulture.Add(typeof(TPropType), culture);
            return (PrintingConfig<TOwner>) parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>
	        (this PropertyPrintingConfig<TOwner, int> config, CultureInfo culture) 
                => SetCultureForType(config, culture);

	    public static PrintingConfig<TOwner> Using<TOwner>
	        (this PropertyPrintingConfig<TOwner, long> config, CultureInfo culture) 
                => SetCultureForType(config, culture);

        public static PrintingConfig<TOwner> Using<TOwner>
	        (this PropertyPrintingConfig<TOwner, double> config, CultureInfo culture) 
                => SetCultureForType(config, culture);

	    public static PrintingConfig<TOwner> Using<TOwner>
	        (this PropertyPrintingConfig<TOwner, float> config, CultureInfo culture) 
                => SetCultureForType(config, culture);

    }
}