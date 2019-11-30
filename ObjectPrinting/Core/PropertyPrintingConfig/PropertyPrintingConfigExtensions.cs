using System;
using System.Globalization;
using ObjectPrinting.Core.PrintingConfig;

namespace ObjectPrinting.Core.PropertyPrintingConfig
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
            if (maxLen < 0)
            {
                throw new ArgumentException("MaxLen should be non-negative");
            }

            return propConfig.Using(str =>
                str.Length > maxLen
                    ? str.Substring(0, maxLen)
                    : str);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, byte> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, sbyte> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ushort> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, uint> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> propConfig,
            CultureInfo cultureInfo)
        {
            return UsingCultureForNumberType(propConfig, cultureInfo);
        }

        private static PrintingConfig<TOwner> UsingCultureForNumberType<TOwner, TPropType>(
            PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo cultureInfo)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner>) propConfig).ParentConfig;
            ((IPrintingConfig) parentConfig).TypeCultures[typeof(TPropType)] = cultureInfo;
            return parentConfig;
        }
    }
}