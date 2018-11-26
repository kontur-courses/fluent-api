using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig, CultureInfo culture)
        {
            var printingConfig = (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, int>).PrintingConfig;
            printingConfig.AddCultureInfo(typeof(int), culture);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> propertyPrintingConfig, CultureInfo culture)
        {
            var printingConfig = (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, double>).PrintingConfig;
            printingConfig.AddCultureInfo(typeof(double), culture);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, float> propertyPrintingConfig, CultureInfo culture)
        {
            var printingConfig = (propertyPrintingConfig as IPropertyPrintingConfig<TOwner, float>).PrintingConfig;
            printingConfig.AddCultureInfo(typeof(float), culture);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int trimLength)
        {
            var iPropertyPrintingConfig = propertyPrintingConfig as IPropertyPrintingConfig<TOwner, string>;
            var printingConfig = iPropertyPrintingConfig.PrintingConfig;
            var propertyName = iPropertyPrintingConfig.PropertyName;
            Func<string, string> trimFunc = s => s.Substring(0, Math.Min(trimLength, s.Length - 1));
            printingConfig.AddAlternativePropertySerializer(trimFunc, propertyName);
            return printingConfig;
        }
    }
}