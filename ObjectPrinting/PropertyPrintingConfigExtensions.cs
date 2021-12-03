using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (maxLen < 0)
            {
                throw new ArgumentException("Max length should be non-negative");
            }

            var config = (IPropertyPrintingConfig<TOwner, string>)propConfig;
            config.ParentConfig.StringPropertyToLength[config.PropertyInfo] = maxLen;
            return config.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TProp>(this PropertyPrintingConfig<TOwner, TProp> propConfig,
            CultureInfo culture) where TProp : IFormattable
        {
            var config = (IPropertyPrintingConfig<TOwner, TProp>)propConfig;
            config.ParentConfig.TypeToCultureInfo[typeof(TProp)] = culture;
            return config.ParentConfig;
        }
    }
}