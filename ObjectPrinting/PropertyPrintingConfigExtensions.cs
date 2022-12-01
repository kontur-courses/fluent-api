using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> With<TOwner, TField>(
            this PropertyPrintingConfig<TOwner, TField> propertyPrintingConfig,
            CultureInfo culture)
            where TField : IFormattable
        {
            propertyPrintingConfig.With(t => t.ToString(format: null, culture));
            return propertyPrintingConfig.parentConfig;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propPrintingConfig, int maxLength)
        {
            propPrintingConfig.With(s => s.Substring(0, maxLength));

            if (propPrintingConfig.parentConfig != null)
                return propPrintingConfig.parentConfig;

            throw new ArgumentException("parentConfig should not be null");
        }

        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}
