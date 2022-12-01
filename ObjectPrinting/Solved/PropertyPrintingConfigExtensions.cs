using System;

namespace ObjectPrinting.Solved
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
            if (propConfig.PropertyConfig.PropertyType != typeof(string))
                throw new ArgumentException("Property is not string");
            var propertyConfig = (IPropertyPrintingConfig<TOwner, string>)propConfig;
            propertyConfig.ParentConfig.AddMaxLenghtForStringProperty(propConfig.PropertyConfig, maxLen);
            return propertyConfig.ParentConfig;
        }
    }
}