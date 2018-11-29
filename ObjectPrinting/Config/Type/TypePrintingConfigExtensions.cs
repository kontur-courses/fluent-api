using System;
using System.Globalization;

namespace ObjectPrinting.Config.Type
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            var parentConfig = propConfig.ParentConfig;
            parentConfig.OverrideTypeCulture<int>(culture);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            var parentConfig =  propConfig.ParentConfig;
            parentConfig.OverrideTypeCulture<double>(culture);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            var parentConfig = propConfig.ParentConfig;
            parentConfig.OverrideTypeCulture<float>(culture);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, decimal> propConfig, CultureInfo culture)
        {
            var parentConfig = propConfig.ParentConfig;
            parentConfig.OverrideTypeCulture<float>(culture);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, DateTime> propConfig, CultureInfo culture)
        {
            var parentConfig = propConfig.ParentConfig;
            parentConfig.OverrideTypeCulture<DateTime>(culture);

            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, TimeSpan> propConfig, CultureInfo culture)
        {
            var parentConfig = propConfig.ParentConfig;
            parentConfig.OverrideTypeCulture<DateTime>(culture);

            return parentConfig;
        }

        //public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        //{
        //    return config(ObjectPrinter.For<T>()).PrintToString(obj);
        //}
    }
}