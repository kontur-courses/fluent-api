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

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
            where TPropType : IFormattable
        {
            var config = ((IPropertyPrintingConfig<TOwner, TPropType>)propConfig).ParentConfig;
            config.SetCultureForFormattable<TPropType>(culture);
            return config;
        }
    }
}