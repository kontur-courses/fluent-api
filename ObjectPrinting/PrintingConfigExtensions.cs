using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Substring<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int start, int end)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }
    }
}