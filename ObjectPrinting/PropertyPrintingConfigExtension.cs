using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
        
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
                printingConfig.AddSerializationForProperty<string>(
                    propConfig.MemberName, str => str.Substring(0, maxLen));
            return printingConfig;
        }
    }
}