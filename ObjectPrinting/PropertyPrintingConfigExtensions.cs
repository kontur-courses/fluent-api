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
            propConfig.PropertyRule = s => s.Substring(0, maxLen);
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner,TPropType>(this PropertyPrintingConfig<TOwner,TPropType> propConfig, CultureInfo culture)
            where TPropType : IConvertible
        {
            propConfig.PropertyRule = t => t.ToString(culture);
            return ((IPropertyPrintingConfig<TOwner, TPropType>)propConfig).ParentConfig;
        }

    }
}