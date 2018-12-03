using System;
using System.Globalization;
using ObjectPrinting.PrintingInterfaces;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Action<PrintingConfig<T>> configure)
            where T : new()
        {
            var printingConfig = ObjectPrinter.For<T>();
            configure(printingConfig);
            return printingConfig.PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var printingConfig =  ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            var property = (propConfig as IPropertyPrintingConfig<TOwner, string>).Property;
            ((IPrintingConfig) printingConfig).PropertyCustomSerializers.Add(property, new Func<string, string>(v => v?.Substring(0, maxLen) ?? string.Empty));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            var printingConfig = (config as IPropertyPrintingConfig<TOwner, double>).ParentConfig;
            (printingConfig as IPrintingConfig).TypeCustomSerializers.Add(typeof(double), new Func<double, string>(x => $"{x.ToString(culture)} - {culture.DisplayName}"));
            return printingConfig;
        }
    }
}