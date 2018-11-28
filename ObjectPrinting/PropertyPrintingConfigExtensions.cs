using System;

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
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            var propertyNameToTrimValue = ((IPropertyPrintingConfig<TOwner, string>)propConfig).PropertyNameToTrimValue;
            ((IPrintingConfig<TOwner>)parentConfig).PropertiesToTrim.Add(propertyNameToTrimValue, maxLen);
            return parentConfig;
        }

    }
}