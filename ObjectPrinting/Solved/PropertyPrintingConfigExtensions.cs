using System;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            if (propConfig.fullNameProp == null)
                parentConfig.TrimedString(maxLen);
            else
                parentConfig.AddFieldsTrim(propConfig.fullNameProp, maxLen);
            return parentConfig;
        }
    }
}