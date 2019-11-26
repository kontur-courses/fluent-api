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
            Func<string, string> func = x => x.Substring(0, maxLen);
            ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig.SetFuncFor(typeof(string), func );
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
    }
}