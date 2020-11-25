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

        public static PrintingConfig<TOwner> UsingCulture<TOwner, T>(
            this PropertyPrintingConfig<TOwner, T> propConfig,
            CultureInfo culture)
            where T : IFormattable
        {
            return propConfig.Using(x => x.ToString(null, culture));
        }
        
        public static PrintingConfig<TOwner> PrintingConfig<TOwner>(this PropertyPrintingConfig<TOwner,T> propConfig)

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
        }
    }
}