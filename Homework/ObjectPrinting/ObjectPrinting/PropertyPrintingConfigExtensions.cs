using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config,
                                              int serialiseDepth = 30) =>
            config(ObjectPrinter.For<T>(serialiseDepth)).PrintToString(obj);

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxValueLength)
        {
            if (maxValueLength <= 0)
                throw new ArgumentException("Length has to be more than 0.", nameof(maxValueLength));

            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>)propertyConfig).ParentConfig;

            ((IPrintingConfig)printingConfig).SetMaxValueLengthForStringProperty(maxValueLength);

            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propertyConfig,
                                                           CultureInfo cultureInfo) =>
            propertyConfig.UsingCultureInfo(number => number.ToString(cultureInfo));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propertyConfig,
                                                           CultureInfo cultureInfo) =>
            propertyConfig.UsingCultureInfo(number => number.ToString(cultureInfo));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propertyConfig,
                                                           CultureInfo cultureInfo) =>
            propertyConfig.UsingCultureInfo(number => number.ToString(cultureInfo));

        private static PrintingConfig<TOwner> UsingCultureInfo<TOwner, TNumber>(
            this IPropertyPrintingConfig<TOwner, TNumber> propertyConfig, Func<TNumber, string> cultureApplier)
            where TNumber : struct
        {
            ((IPrintingConfig)propertyConfig.ParentConfig).SetCultureInfoApplierForNumberType(cultureApplier);

            return propertyConfig.ParentConfig;
        }
    }
}