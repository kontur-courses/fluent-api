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
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            printingConfig.TypeConverters[typeof(string)] =
                x => (x as string)?.Substring(0, Math.Min(((string) x).Length, maxLen));
            return printingConfig;
        }
    }
}