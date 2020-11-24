using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<Configurator<T>, Configurator<T>> config)
        {
            return Printer<T>.PrintToString(obj, config);
        }

        public static Configurator<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            return parentConfig.AddPrintingMethod((string s) => s.Substring(0, Math.Min(s.Length, maxLen)));
        }
    }
}