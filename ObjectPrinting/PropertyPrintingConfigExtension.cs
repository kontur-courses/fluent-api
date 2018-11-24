using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            propertyPrintingConfig.Using(i => i.ToString(cultureInfo));
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, int>).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            propertyPrintingConfig.Using(d => d.ToString(cultureInfo));
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, double>).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            propertyPrintingConfig.Using(d => d.ToString(cultureInfo));
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, long>).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            propertyPrintingConfig.Using(d => d.ToString(cultureInfo));
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, float>).PrintingConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int prefixLength)
        {
            propertyPrintingConfig.Using(s => s.Substring(0, Math.Min(prefixLength, s.Length)));
            return (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, string>).PrintingConfig;
        }
    }
}
