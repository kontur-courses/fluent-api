using System;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting
{
    public static class PropertyPrintingExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, int>) propConfig).ParentConfig;
            config.AddTypeCulture(typeof(int), cultureInfo);
            return config;
        }  
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {            
            var config = ((IPropertyPrintingConfig<TOwner, double>) propConfig).ParentConfig;
            config.AddTypeCulture(typeof(double), cultureInfo);
            return config;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig,
            CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, float>) propConfig).ParentConfig;
            config.AddTypeCulture(typeof(float), cultureInfo);
            return config;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig,
            CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, long>) propConfig).ParentConfig;
            config.AddTypeCulture(typeof(long), cultureInfo);
            return config;
        }
        
        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
            int length)
        {
            var config = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            var name = ((IPropertyPrintingConfig<TOwner, string>) propConfig).Name;
            // ReSharper disable once ConvertToLocalFunction
            Func<string, string> func = s => string.Concat(s.Take(Math.Min(s.Length, length)));
            if (name != string.Empty)
                config.AddPropertyPrintingSettings(name, func);
            else
            {
                config.AddTypePrintingSettings(typeof(string), func);
            }

            return config;
        }
    }
}