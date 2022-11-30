using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static CustomSerializablePrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            var parentConfig = propConfig.CustomSerializablePrintingConfig;
            if (propConfig.MemberSelector.Body is MemberExpression memberExpression)
                parentConfig.StringPropertyLengths[memberExpression.Member] = maxLength;
            return propConfig.CustomSerializablePrintingConfig;
        }
        
        public static CustomSerializablePrintingConfig<TOwner> Use<TOwner, TProp>(this PropertyPrintingConfig<TOwner, TProp> propertyConfig, CultureInfo culture) where TProp: IFormattable
        {
            var type = propertyConfig.PropertyType;
            var config = propertyConfig.CustomSerializablePrintingConfig;
            var cultures = config.TypesCultures;
            if (cultures.ContainsKey(type))
                cultures[type] = culture;
            else
                cultures.Add(type, culture);
            return config;
        }
    }
}