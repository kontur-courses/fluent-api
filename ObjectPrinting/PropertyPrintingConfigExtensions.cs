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

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(int), culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo culture)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, long>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(int), culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(int), culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> propConfig, CultureInfo culture)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, decimal>)propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(int), culture);
            return parentConfig;
        }
    }
}