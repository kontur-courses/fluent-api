using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
        }

        private static PrintingConfig<TOwner> Using<TOwner, TProperty>(this PropertyPrintingConfig<TOwner, TProperty> propConfig,
            CultureInfo cultureInfo)
        {
            var parentConfig = (IPrintingConfig)((IPropertyPrintingConfig<TOwner, double>) propConfig).ParentConfig;
            parentConfig.TypesCultures[typeof(TProperty)] = cultureInfo;
            return ((IPropertyPrintingConfig<TOwner, double>) propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {
            return propConfig.Using<TOwner, double>(cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo cultureInfo)
        {
            return propConfig.Using<TOwner, int>(cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig,
            CultureInfo cultureInfo)
        {
            return propConfig.Using<TOwner, float>(cultureInfo);
        }
    }
}