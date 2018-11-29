using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {     
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.Using(n => n.ToString().Substring(0,maxLen));
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> ChangeCultureInfo<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            propConfig.Using(n => n.ToString(culture));
            return ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> ChangeCultureInfo<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            propConfig.Using(n => n.ToString(culture));
            return ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> ChangeCultureInfo<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo culture)
        {
            propConfig.Using(n => n.ToString(culture));
            return ((IPropertyPrintingConfig<TOwner, long>)propConfig).ParentConfig;
        }

    }
}