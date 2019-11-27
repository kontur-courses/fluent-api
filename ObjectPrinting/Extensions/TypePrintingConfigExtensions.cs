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

        private static PrintingConfig<TOwner> Using<TOwner, TProperty>(this TypePrintingConfig<TOwner, TProperty> propConfig,
            CultureInfo cultureInfo)
        {
            var parentConfig = (IPrintingConfig)((IPropertyPrintingConfig<TOwner, double>) propConfig).ParentConfig;
            parentConfig.TypesCultures[typeof(TProperty)] = cultureInfo;
            return ((IPropertyPrintingConfig<TOwner, double>) propConfig).ParentConfig;
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