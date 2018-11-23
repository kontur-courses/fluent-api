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
            var config = (IPropertyPrintingConfig<TOwner>) propConfig;
            config.PrintingFunction = obj =>
            {
                var str = (string) obj;
                return str.Length > maxLen ? str.Substring(0, maxLen) : str;
            };
            return config.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            return propConfig.SetCulture(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            return propConfig.SetCulture(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            return propConfig.SetCulture(culture);
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo culture)
        {
            return propConfig.SetCulture(culture);
        }

        private static PrintingConfig<TOwner> SetCulture<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
        {
            var config = (IPropertyPrintingConfig<TOwner>)propConfig;
            config.Culture = culture;
            return config.ParentConfig;
        }
    }
}