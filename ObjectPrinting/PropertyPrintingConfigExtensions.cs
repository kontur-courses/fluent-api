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

        public static PrintingConfig<TOwner> SetMaxLength<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig, int maxLen)
        {
            var propertyConfig = ((IPropertyPrintingConfig<TOwner, TPropType>) propConfig).ParentConfig;

            propertyConfig.SetMaxLength<TPropType>(maxLen);
            return propertyConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
            where TPropType : IFormattable
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, TPropType>) propConfig).ParentConfig;
            printingConfig.SetCulture<TPropType>(culture);
            return printingConfig;
        }
    }
}