using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig,
            int maxLen)
        {
            var config = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            config.AddCutProperty(propConfig.PropertyName, maxLen);
            return config;
        }
    }
}