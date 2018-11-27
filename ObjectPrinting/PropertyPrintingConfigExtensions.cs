using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture) => propConfig.UsingCulture(culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture) => propConfig.UsingCulture(culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo culture) => propConfig.UsingCulture(culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> propConfig, CultureInfo culture) => propConfig.UsingCulture(culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture) => propConfig.UsingCulture(culture);

        private static PrintingConfig<TOwner> UsingCulture<TOwner, TNumericType>(this PropertyPrintingConfig<TOwner, TNumericType> propConfig, CultureInfo culture)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, TNumericType>)propConfig).ParentConfig;
            printingConfig.AddCulture<TNumericType>(culture);
            return printingConfig;
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig);
            var parentConfig = printingConfig.ParentConfig;
            var propertyName = printingConfig.PropertyName ?? throw new ArgumentException();
            parentConfig.AddPropertyTrimm(propertyName, maxLen);
            return parentConfig;
        }

    }
}