using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {     
        public static PropertyPrintingConfig<TOwner,string> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            ((IPropertyPrintingConfig<string>)propConfig).PrintMethods.Add(s =>
            {
                return ((string)s).Length > maxLen ? ((string)s).Substring(0, maxLen) : (string)s;
            });
            return propConfig;
        }

        public static PropertyPrintingConfig<TOwner, IFormattable> ChangeCultureInfo<TOwner>(this PropertyPrintingConfig<TOwner, IFormattable> propConfig, CultureInfo culture)
        {
            ((IPropertyPrintingConfig<IFormattable>)propConfig).PrintMethods.Add(fo => ((IFormattable)fo).ToString(culture.Name, culture));
            return propConfig;
        }
    }
}