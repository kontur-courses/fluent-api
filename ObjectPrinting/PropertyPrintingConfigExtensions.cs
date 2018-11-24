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
            this PropertyPrintingConfig<TOwner, string> propConfig,
            int maxLen)
        {
            var config = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            config.AddCutProperty(propConfig.PropertyName, maxLen);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, double>) propertyPrintingConfig).ParentConfig;
            config.AddTypeSerializationCulture(typeof(double), cultureInfo);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, int>) propertyPrintingConfig).ParentConfig;
            config.AddTypeSerializationCulture(typeof(int), cultureInfo);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, long>) propertyPrintingConfig).ParentConfig;
            config.AddTypeSerializationCulture(typeof(long), cultureInfo);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, float> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, float>) propertyPrintingConfig).ParentConfig;
            config.AddTypeSerializationCulture(typeof(float), cultureInfo);
            return config;
        }
    }
}