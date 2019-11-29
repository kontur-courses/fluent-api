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
            propConfig.Using(p => p.Substring(0, Math.Min(maxLen, p.Length)));
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo cultureInfo)
        {
            propConfig.Using(p => p.ToString(cultureInfo));
            return ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {
            propConfig.Using(p => p.ToString(cultureInfo));
            return ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
        }
    }
}