using System;
using System.Collections;
using System.Globalization;

namespace ObjectPrinting
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
            propConfig.AddPrintingFunction(property => property.Length > maxLen 
                ? property.Substring(0, maxLen) 
                : property);
            return propConfig.PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
            where TPropType: IFormattable
        {
            propConfig.AddCulture(culture);
            return propConfig.PrintingConfig;
        }

        public static PrintingConfig<TOwner> WithItemsMaximum<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig, int maxLength)
            where TPropType: IEnumerable
        {
            if (maxLength < 0)
                throw new ArgumentException("maxLength can't be negative");
            propConfig.PrintingConfig.MaxCollectionLength = maxLength;
            return propConfig.PrintingConfig;
        }
    }
}