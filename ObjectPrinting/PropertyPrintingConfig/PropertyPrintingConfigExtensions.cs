using System;
using System.Globalization;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting.PropertyPrintingConfig
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> WithCulture<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner,TPropType> propPrintingConfig, CultureInfo culture)
            where TPropType : IFormattable
        {
            propPrintingConfig.WithConfig(type => type.ToString(null, culture));
            return propPrintingConfig.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propPrintingConfig, int maxLen)
        {
            propPrintingConfig.WithConfig(str => str.Substring(0, maxLen));
            return propPrintingConfig.ParentConfig;
        }
        
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}