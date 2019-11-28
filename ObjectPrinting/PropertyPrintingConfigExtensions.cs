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

        #region ExtensionsUsingWithCultureForNumbers

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, sbyte> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, byte> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ushort> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, uint> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> propConfig,
            CultureInfo culture) => AddCulture(propConfig, culture);

        #endregion

        private static PrintingConfig<TOwner> AddCulture<TOwner, TNumber>(
            PropertyPrintingConfig<TOwner, TNumber> propConfig,
            CultureInfo culture)
        {
            var propertyConfig = (IPropertyPrintingConfig<TOwner, TNumber>) propConfig;
            var parentConfig = propertyConfig.ParentConfig;
            var printingConfig = (IPrintingConfig) parentConfig;
            printingConfig.CultureForNumber[typeof(TNumber)] = culture;
            return parentConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentException("maxLength must be non-negative");
            var propertyConfig = (IPropertyPrintingConfig<TOwner, string>) propConfig;
            var parentConfig = propertyConfig.ParentConfig;
            var propertyInfo = propertyConfig.PropertyInfo;
            var printingConfig = (IPrintingConfig) parentConfig;
            printingConfig.PropertyPrintingMethod[propertyInfo] =
                (Func<string, string>) (s => s.Substring(0, Math.Min(maxLength, s.Length)));
            return parentConfig;
        }
    }
}