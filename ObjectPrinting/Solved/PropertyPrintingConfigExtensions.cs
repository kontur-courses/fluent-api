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
            propConfig.Using(s => s[0..Math.Min(s.Length, maxLen)]);
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
    }
}