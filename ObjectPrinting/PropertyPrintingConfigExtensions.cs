using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo cultureInfo)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int maxLen)
        {
            return ((IPropertyPrintingConfig<TOwner>) config).ParentConfig;
        }
    }
}