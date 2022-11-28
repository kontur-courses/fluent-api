using System;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxLength)
        {
            propertyConfig.AlternativePrint = str => str[..maxLength];
            return ((IPropertyPrintingConfig<TOwner, string>)propertyConfig).ParentConfig;
        }
    }
}