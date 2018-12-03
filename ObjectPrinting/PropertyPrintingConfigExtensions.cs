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
            ((IPrintingConfig)((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig).MaxLength =
                maxLen;
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType> (this PropertyPrintingConfig<TOwner, TPropType> printingConfig, CultureInfo culture)
            where TPropType : IFormattable
        {
            var parentConfig =((IPropertyPrintingConfig<TOwner, TPropType>) printingConfig).ParentConfig;
            ((IPrintingConfig)parentConfig).CultureInfoForTypes[typeof(TPropType)] = culture;
            return parentConfig;
        }
    }
}