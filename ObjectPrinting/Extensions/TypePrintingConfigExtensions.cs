using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class TypePrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        private static PrintingConfig<TOwner> Using<TOwner, TProperty>(this IPropertyPrintingConfig<TOwner> propConfig,
            CultureInfo cultureInfo)
        {
            var parentConfig = (IPrintingConfig)propConfig.ParentConfig;
            var settings = parentConfig.Settings;
            return new PrintingConfig<TOwner>(settings.AddTypeCulture(typeof(TProperty), cultureInfo));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {
            return propConfig.Using<TOwner, double>(cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> propConfig,
            CultureInfo cultureInfo)
        {
            return propConfig.Using<TOwner, int>(cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, float> propConfig,
            CultureInfo cultureInfo)
        {
            return propConfig.Using<TOwner, float>(cultureInfo);
        }
    }
}