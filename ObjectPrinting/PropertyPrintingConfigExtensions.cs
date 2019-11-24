using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo culture)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, int>) propConfig).ParentConfig;
            var fieldInfo =
                typeof(PrintingConfig<TOwner>).GetField("cultureTypes", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (Dictionary<Type, Func<object, string>>) fieldInfo.GetValue(printingConfig);
            result[typeof(int)] = x => ((int) x).ToString(culture);
            return printingConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo culture)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, double>) propConfig).ParentConfig;
            var fieldInfo =
                typeof(PrintingConfig<TOwner>).GetField("cultureTypes", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (Dictionary<Type, Func<object, string>>) fieldInfo.GetValue(printingConfig);
            result[typeof(double)] = x => ((double) x).ToString(culture);
            return printingConfig;
        }
    }
}