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
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, string>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForMembers[interfacedPropConfig.Member] =
                o => ((string)o).Substring(0, ((string)o).Length < maxLen ? ((string)o).Length : maxLen);
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TProp>(this PropertyPrintingConfig<TOwner, TProp> propConfig,
            CultureInfo culture) where TProp : IFormattable
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, TProp>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(TProp)] = o => ((TProp)o).ToString("", culture);
            return interfacedPropConfig.ParentConfig;
        }
    }
}