using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            var printingConfig = (config as IPropertyPrintingConfig<TOwner, int>).PrintingConfig;
            printingConfig.Cultures.Add(typeof(int), culture);
            return printingConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> config,
            CultureInfo culture)
        {
            var printingConfig = (config as IPropertyPrintingConfig<TOwner, float>).PrintingConfig;
            printingConfig.Cultures.Add(typeof(float), culture);
            return printingConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> config,
            CultureInfo culture)
        {
            var printingConfig = (config as IPropertyPrintingConfig<TOwner, double>).PrintingConfig;
            printingConfig.Cultures.Add(typeof(double), culture);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> CutToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int length)
        {
            var cut = new Func<string, string>(s => s.Substring(0, Math.Min(s.Length, length)));
            var property = (config as IPropertyPrintingConfig<TOwner, string>).Property;
            var printingConfig = (config as IPropertyPrintingConfig<TOwner, string>).PrintingConfig;
            printingConfig.PropertyToCut.Add(property, cut);
            return  printingConfig;
        }
        
        public static string PrintToString<T>(this T printedObject)
        {
            return ObjectPrinter.For<T>().PrintToString(printedObject);
        }
        
        public static string PrintToString<T>(this T printedObject, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(printedObject);
        }
    }
}