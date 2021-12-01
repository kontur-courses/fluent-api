using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
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