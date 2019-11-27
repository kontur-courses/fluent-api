using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxLen)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>)propertyConfig).ParentConfig;
            var property = ((IPropertyPrintingConfig<TOwner, string>)propertyConfig).PropertyInfo;
            ((IPrintingConfig<TOwner>)printingConfig).PropertyTrimmers[property] = maxLen;
            return printingConfig;
        }

    }
}