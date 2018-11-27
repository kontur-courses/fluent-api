using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {     
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            return ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
        }

    }
}