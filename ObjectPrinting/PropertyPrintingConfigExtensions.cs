using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            if (propConfig.MemberSelector is { Body: MemberExpression memberExpression })
                parentConfig.StringPropertyLengths[memberExpression.Member] = maxLength;
            else
            {
                var type = typeof(TOwner);
                var members = type.GetProperties().Where(x => x.PropertyType == typeof(string))
                    .Concat<MemberInfo>(type.GetFields().Where(x => x.FieldType == typeof(string)));
                foreach (var info in members)
                    parentConfig.StringPropertyLengths[info] = maxLength;
            }

            return propConfig.PrintingConfig;
        }

        public static PrintingConfig<TOwner> Use<TOwner, TProp>(
            this PropertyPrintingConfig<TOwner, TProp> propertyConfig, CultureInfo culture) where TProp : IFormattable
        {
            var type = propertyConfig.PropertyType;
            var config = propertyConfig.PrintingConfig;
            var cultures = config.TypeCultures;
            if (cultures.ContainsKey(type))
                cultures[type] = culture;
            else
                cultures.Add(type, culture);
            return config;
        }
    }
}