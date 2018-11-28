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
            parentConfig.ChangeSerializationForType(typeof(string), 
                new Func<string, string>(s => s.Substring(0, Math.Min(maxLen, s.Length-1))));
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo cultureInfo)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
            parentConfig.ChangeCultureForType(typeof(int), cultureInfo);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo cultureInfo)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
            parentConfig.ChangeCultureForType(typeof(double), cultureInfo);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo cultureInfo)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, long>)propConfig).ParentConfig;
            parentConfig.ChangeCultureForType(typeof(long), cultureInfo);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo cultureInfo)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, float>)propConfig).ParentConfig;
            parentConfig.ChangeCultureForType(typeof(float), cultureInfo);
            return parentConfig;
        }

    }
}