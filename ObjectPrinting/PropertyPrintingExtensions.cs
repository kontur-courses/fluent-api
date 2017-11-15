using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo cultureInfo)
        {
            return ((IPropertyPrintingConfig<TOwner, int>) config).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(
            this PropertyPrintingConfig<TOwner, long> config,
            CultureInfo cultureInfo)
        {
            return ((IPropertyPrintingConfig<TOwner, long>) config).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(
            this PropertyPrintingConfig<TOwner, double> config,
            CultureInfo cultureInfo)
        {
            return ((IPropertyPrintingConfig<TOwner, double>) config).ParentConfig;
        }

        public static PrintingConfig<TOwner> ShrinkToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config,
            int length)
        {
            return ((IPropertyPrintingConfig<TOwner, string>) config).ParentConfig;
        }
    }
}