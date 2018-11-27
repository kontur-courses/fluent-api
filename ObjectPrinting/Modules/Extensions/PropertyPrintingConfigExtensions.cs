using System;
using System.Globalization;
using ObjectPrinting.Modules.PrintingConfig;

namespace ObjectPrinting.Modules.Extensions
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var propertyPrintingConfig = (IPropertyPrintingConfig<TOwner, string>)propConfig;
            var config = propertyPrintingConfig.ParentConfig;
            var propertyName = propertyPrintingConfig.PropertyName;
            config.SetTrimmingLength(propertyName, maxLen);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            var config = ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
            config.SetTypeCulture<double>(culture);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            var config = ((IPropertyPrintingConfig<TOwner, float>)propConfig).ParentConfig;
            config.SetTypeCulture<float>(culture);
            return config;
        }
    }
}