using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo cultureInfo)
        {
            return AddCulture(propConfig, cultureInfo);
        }
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {
            return AddCulture(propConfig, cultureInfo);
        }
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, float> propConfig,
            CultureInfo cultureInfo)
        {
            return AddCulture(propConfig, cultureInfo);
        }

        private static PrintingConfig<TOwner> AddCulture<TOwner, TPropType>(
            IPropertyPrintingConfig<TOwner, TPropType> propConfig,
            CultureInfo cultureInfo)
        {
            var config = propConfig.PrintingConfig;
            ((IPrintingConfig)config).AddCultureInfo(typeof(TPropType), cultureInfo);
            return config;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
            int count)
        {
            var iPropConfig = (IPropertyPrintingConfig<TOwner, string>) propConfig;
            var propertySelector = iPropConfig.PropertySelector;
            if (!(propertySelector.Body is MemberExpression body))
                throw new ArgumentException();
            var propertyName = body.Member.Name;
            var property = typeof(TOwner).GetProperty(propertyName);

            string Trim(object s)
            {
                var strS = (string) s;
                return count > strS.Length ? strS : strS.Substring(0, count);
            }

            var config = iPropConfig.PrintingConfig;
            ((IPrintingConfig)config).AddPostProduction(property, Trim);
            return config;
        }
    }
}
