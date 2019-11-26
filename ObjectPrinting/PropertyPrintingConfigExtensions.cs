using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo cultureInfo)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
            ((IPrintingConfig) parent).CultureLookup[typeof(int)] = cultureInfo;
            return parent;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int maxLen)
        {
            config.Using(s => s.Substring(0, Math.Min(s.Length, maxLen)));
            return ((IPropertyPrintingConfig<TOwner>) config).ParentConfig;
        }
    }
}