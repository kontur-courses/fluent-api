using System;
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
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo cultureInfo)
        {
            return AddCulture(propConfig, cultureInfo);
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {
            return AddCulture(propConfig, cultureInfo);
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig,
            CultureInfo cultureInfo)
        {
            return AddCulture(propConfig, cultureInfo);
        }

        private static PrintingConfig<TOwner> AddCulture<TOwner>(IPropertyPrintingConfig<TOwner> propConfig,
            CultureInfo cultureInfo)
        {
            var config = propConfig.PrintingConfig;
            ((IPrintingConfig<TOwner>)config).AddCultureInfo(typeof(TOwner), cultureInfo);
            return config;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
            int count)
        {
            if (!(propConfig.propertySelector.Body is MemberExpression body))
                throw new ArgumentException();
            var propertyName = body.Member.Name;
            var property = typeof(TOwner).GetProperty(propertyName);

            string Trim(string s) => count > s.Length ? s : s.Substring(0, count);

            var config = ((IPropertyPrintingConfig<TOwner>) propConfig).PrintingConfig;
            ((IPrintingConfig<TOwner>)config).AddPostProduction(property, (Func<string, string>) Trim);
            return config;
        }
    }
}
