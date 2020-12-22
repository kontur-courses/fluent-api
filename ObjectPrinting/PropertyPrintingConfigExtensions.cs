using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner, TProperty>(
            this PropertyPrintingConfig<TOwner, TProperty> propertyPrintingConfig, CultureInfo culture)
            where TProperty : IFormattable
        {
            var newConfig = new PrintingConfig<TOwner>(propertyPrintingConfig.ParentPrintingConfig);
            newConfig.CulturesForProperties[propertyPrintingConfig.Property] = culture;
            return newConfig;
        }
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            return 
        }
    }
}
