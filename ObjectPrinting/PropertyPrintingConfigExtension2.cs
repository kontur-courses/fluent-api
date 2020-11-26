using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension2
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            var propertyConfig = (IPropertyPrintingConfig<TOwner, int>) propConfig;
            propertyConfig.ParentConfig.AddCultureForNumber(typeof(int), culture);
            return propertyConfig.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            var propertyConfig = (IPropertyPrintingConfig<TOwner, double>) propConfig;
            propertyConfig.ParentConfig.AddCultureForNumber(typeof(double), culture);
            return propertyConfig.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            var propertyConfig = (IPropertyPrintingConfig<TOwner, float>) propConfig;
            propertyConfig.ParentConfig.AddCultureForNumber(typeof(float), culture);
            return propertyConfig.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, DateTime> propConfig, CultureInfo culture)
        {
            var propertyConfig = (IPropertyPrintingConfig<TOwner, DateTime>) propConfig;
            propertyConfig.ParentConfig.AddCultureForNumber(typeof(DateTime), culture);
            return propertyConfig.ParentConfig;
        }
    }
}