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

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            ((IPrintingConfig<TOwner>) ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig)
                .StringPropNamesTrimming[((IPropertyPrintingConfig<TOwner, string>) propConfig).PropertyName] = maxLen;

            return ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>
            (this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig, CultureInfo culture)
        {
            propertyPrintingConfig.SetCulture(typeof(int), culture);
            return ((IPropertyPrintingConfig<TOwner, int>) propertyPrintingConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>
            (this PropertyPrintingConfig<TOwner, double> propertyPrintingConfig, CultureInfo culture)
        {
            propertyPrintingConfig.SetCulture(typeof(double), culture);
            return ((IPropertyPrintingConfig<TOwner, double>) propertyPrintingConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>
            (this PropertyPrintingConfig<TOwner, long> propertyPrintingConfig, CultureInfo culture)
        {
            propertyPrintingConfig.SetCulture(typeof(long), culture);
            return ((IPropertyPrintingConfig<TOwner, long>) propertyPrintingConfig).ParentConfig;
        }
    }
}