using System;
using System.Globalization;

namespace ObjectPrinting.Solved
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
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }


        private static PrintingConfig<TOwner> Using<TOwner, TProType>(
            this IPropertyPrintingConfig<TOwner, TProType> propertyPrintingConfig,
            CultureInfo cultureInfo)
            where TProType : struct, IFormattable
        {
            propertyPrintingConfig.ParentConfig.typeCultures.Add(typeof(TProType), cultureInfo);

            return propertyPrintingConfig.ParentConfig;
        }
    }
}