using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj, Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
        {
            return config(ObjectPrinter.For<TOwner>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            ((NumberPropertyConfig<TOwner, int>) config).CultureInfo = cultureInfo;
            return ((IPropertyPrintingConfig<TOwner>) config).Config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> config, CultureInfo cultureInfo)
        {
            ((NumberPropertyConfig<TOwner, long>) config).CultureInfo = cultureInfo;
            return ((IPropertyPrintingConfig<TOwner>) config).Config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> config, CultureInfo cultureInfo)
        {
            ((NumberPropertyConfig<TOwner, float>) config).CultureInfo = cultureInfo;
            return ((IPropertyPrintingConfig<TOwner>) config).Config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> config, CultureInfo cultureInfo)
        {
            ((NumberPropertyConfig<TOwner, double>) config).CultureInfo = cultureInfo;
            return ((IPropertyPrintingConfig<TOwner>) config).Config;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int length)
        {
            ((StringPropertyConfig<TOwner, string>) config).TrimLength = length;
            return ((IPropertyPrintingConfig<TOwner>) config).Config;
        }
    }
}