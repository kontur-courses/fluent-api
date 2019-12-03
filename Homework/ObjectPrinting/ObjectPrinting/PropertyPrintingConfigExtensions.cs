using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config,
                                              int serialiseDepth = ObjectPrinter.DefaultSerialiseDepth) =>
            config(ObjectPrinter.For<T>(serialiseDepth)).PrintToString(obj);

        public static PrintingConfig<TOwner> Trim<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxContentLength)
        {
            if (maxContentLength <= 0)
                throw new ArgumentException("Length has to be more than 0.", nameof(maxContentLength));

            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>)propertyConfig).ParentConfig;

            ((IPrintingConfig)printingConfig).SetMaxValueLengthForStringProperty(maxContentLength);

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