using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentException("Length can't be negative");

            var parentConfig = propConfig.PrintingConfig;
            if (propConfig.MemberSelector.Body is MemberExpression memberExpression)
                parentConfig.StringPropertyLengths[memberExpression.Member] = maxLength;
            return propConfig.PrintingConfig;
        }

        public static PrintingConfig<TOwner> Use<TOwner, TProp>(
            this PropertyPrintingConfig<TOwner, TProp> propertyConfig, CultureInfo culture) where TProp : IFormattable
        {
            var type = propertyConfig.PropertyType;
            var config = propertyConfig.PrintingConfig;
            var cultures = config.TypesCultures;
            if (cultures.ContainsKey(type))
                cultures[type] = culture;
            else
                cultures.Add(type, culture);
            return config;
        }
    }
}