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

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.Using(s =>
            {
                var length = Math.Min(s.Length, maxLen);
                return s.Substring(0, length);
            });
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, sbyte> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, uint> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ushort> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, byte> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            return UsingCultureForNumberType(propConfig, culture);
        }

        private static PrintingConfig<TOwner> UsingCultureForNumberType<TOwner, TPropType>(PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
        {
            propConfig.Using(n => Convert.ToString(n, culture));
            return ((IPropertyPrintingConfig<TOwner, TPropType>) propConfig).ParentConfig;
        }
    }
}
