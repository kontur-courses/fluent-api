using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> CutToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, double>) propertyPrintingConfig).ParentConfig;
            config.AddCultureForType(typeof(double), cultureInfo);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, float> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, float>) propertyPrintingConfig).ParentConfig;
            config.AddCultureForType(typeof(float), cultureInfo);
            return config;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, long>) propertyPrintingConfig).ParentConfig;
            config.AddCultureForType(typeof(long), cultureInfo);
            return config;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            var config = ((IPropertyPrintingConfig<TOwner, int>) propertyPrintingConfig).ParentConfig;
            config.AddCultureForType(typeof(int), cultureInfo);
            return config;
        }
        
        public static PrintingConfig<TOwner> CutToLenght<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int lenght)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>) propertyPrintingConfig).ParentConfig;
            printingConfig.CuttedStringPropertyAdd(printingConfig, propertyPrintingConfig.PropertyName, lenght);
            return printingConfig;
        }
    }
}